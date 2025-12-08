//Authentication State + API Client
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MIDTIER.Models;
using static System.Net.WebRequestMethods;

namespace StudentCourseEnrollments.Services
{
    public class APIService
    {
        private readonly HttpClient _http;
        private readonly AuthenticationStateProvider _authState;
        private readonly ILocalStorageService _localStorage;

        public APIService(HttpClient http, AuthenticationStateProvider authState, ILocalStorageService localStorage)
        {
            _http = http;
            _authState = authState;
            _localStorage = localStorage;
        }

        private async Task AddToken()
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
        public async Task<T?> TSendAsync<T>(string url, object? data = null, string method = "GET")
        {
            await AddToken();

            HttpResponseMessage response = method.ToUpper() switch
            {
                "GET" => await _http.GetAsync(url),

                "POST" => await _http.PostAsJsonAsync(url, data),

                "PUT" => await _http.PutAsJsonAsync(url, data),

                "DELETE" => await _http.DeleteAsync(url),

                "PATCH" => await SendWithContent(HttpMethod.Patch, url, data),

                "HEAD" => await _http.SendAsync(new HttpRequestMessage(HttpMethod.Head, url)),

                "OPTIONS" => await _http.SendAsync(new HttpRequestMessage(HttpMethod.Options, url)),

                "TRACE" => await _http.SendAsync(new HttpRequestMessage(HttpMethod.Trace, url)),

                "CONNECT" => throw new NotSupportedException("CONNECT is not supported"),

                _ => throw new ArgumentException($"Invalid HTTP method: {method}")
            };

            if (!response.IsSuccessStatusCode)
            {
                // Optional: log response.StatusCode
                return default;
            }

            if (method is "HEAD" or "OPTIONS" or "TRACE")
            {
                return Activator.CreateInstance<T>();
            }

            var result = await response.Content.ReadFromJsonAsync<T>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? Activator.CreateInstance<T>();
        }

        //PATCH METHOD HELPER
        private async Task<HttpResponseMessage> SendWithContent(HttpMethod method, string url, object? data)
        {
            var request = new HttpRequestMessage(method, url);
            if (data != null)
            {
                request.Content = JsonContent.Create(data);
            }
            return await _http.SendAsync(request);
        }
    }
}
