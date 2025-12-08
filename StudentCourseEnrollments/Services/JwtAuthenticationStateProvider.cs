using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using System.Net.Http.Json;

namespace StudentCourseEnrollments.Services
{
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;
        private readonly ILogger<JwtAuthenticationStateProvider> _logger;

        private static readonly TimeSpan _tokenRefreshThreshold = TimeSpan.FromMinutes(5);

        public JwtAuthenticationStateProvider(
            ILocalStorageService localStorage,
            HttpClient http,
            ILogger<JwtAuthenticationStateProvider> logger)
        {
            _localStorage = localStorage;
            _http = http;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var savedToken = await _localStorage.GetItemAsync<string>("token");

            if (string.IsNullOrWhiteSpace(savedToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            //heck if token is expired locally (fast fail)
            if (IsTokenExpired(savedToken))
            {
                await ClearAuthDataAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            //Set Authorization header so protected API calls work
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);

            //Validate token with server (this catches revoked/expired tokens)
            try
            {
                var response = await _http.GetAsync("api/auth/validate");

                if (!response.IsSuccessStatusCode)
                {
                    await ClearAuthDataAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Optional: refresh user info from server
                var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>();
                var claims = BuildClaims(savedToken, userInfo);
                var identity = new ClaimsIdentity(claims, "jwt");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed. Clearing auth state.");
                await ClearAuthDataAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        private async Task ClearAuthDataAsync()
        {
            await _localStorage.RemoveItemAsync("token");
            await _localStorage.RemoveItemAsync("role");
            _http.DefaultRequestHeaders.Authorization = null;
        }

        private bool IsTokenExpired(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null && keyValuePairs.TryGetValue("exp", out var expValue))
            {
                var expSeconds = Convert.ToInt64(expValue.ToString());
                var expiration = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
                return expiration < DateTimeOffset.UtcNow.Add(_tokenRefreshThreshold);
            }
            return true; // if no exp claim → treat as expired
        }

        private List<Claim> BuildClaims(string token, UserInfo? userInfo)
        {
            var claims = ParseClaimsFromJwt(token).ToList();

            // Ensure standard claims exist
            if (userInfo != null)
            {
                claims.RemoveAll(c => c.Type == ClaimTypes.Name);
                claims.RemoveAll(c => c.Type == ClaimTypes.NameIdentifier);
                claims.RemoveAll(c => c.Type == ClaimTypes.Role);

                claims.Add(new Claim(ClaimTypes.NameIdentifier, userInfo.Id));
                claims.Add(new Claim(ClaimTypes.Name, userInfo.Name));
                claims.Add(new Claim(ClaimTypes.Role, userInfo.Role));
            }

            return claims;
        }

        public class UserInfo { public string Id { get; set; } public string Name { get; set; } public string Role { get; set; } };

        //Notify user auth success
        public void NotifyUserAuthentication(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
        //Notifies user through claims logout
        public void NotifyUserLogout()
        {
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        }

        //Parses Claims
        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            var claims = new List<Claim>();

            if (keyValuePairs != null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    if (kvp.Value is System.Text.Json.JsonElement element)
                    {
                        if (element.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            foreach (var item in element.EnumerateArray())
                            {
                                claims.Add(new Claim(kvp.Key, item.ToString()));
                            }
                        }
                        else
                        {
                            claims.Add(new Claim(kvp.Key, element.ToString()));
                        }
                    }
                }

                // Add name claim if not present
                if (!claims.Any(c => c.Type == ClaimTypes.Name))
                {
                    var nameClaim = claims.FirstOrDefault(c => c.Type == "name" || c.Type.Contains("name"));
                    if (nameClaim != null)
                        claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));
                }
            }

            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
