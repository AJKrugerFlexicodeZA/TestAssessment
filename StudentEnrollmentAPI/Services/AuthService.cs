using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MIDTIER.Models;
using StudentCourseEnrollments.Services.StudentCourseEnrollments.Services;
using StudentEnrollmentAPI.Data;
using StudentEnrollmentAPI.Interfaces;

namespace StudentEnrollmentAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(IConfiguration config, IPasswordHasher<User> passwordHasher)
        {
            _config = config;
            _passwordHasher = passwordHasher;
        }
        public AuthResponse Login(Login request)
        {
            //Basic validation
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new AuthResponse { Message = "Email and password are required", Code = 400 };
            }

            // Find user by email (case-insensitive)
            var user = DataStore.Users.Values
                .FirstOrDefault(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

            //User not found
            if (user == null)
            {
                LogService.Warn("Login failed: User not found", "Auth", 0, request.Email);
                return new AuthResponse { Message = "Invalid credentials", Code = 401 };
            }

            //Account is disabled
            if (!user.IsActive)
            {
                LogService.Warn($"Login blocked: Account is inactive", "Auth", user.Id, request.Email);
                return new AuthResponse { Message = "Account is disabled. Contact administrator.", Code = 401 };
            }

            //Wrong password
            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                LogService.Warn("Login failed: Invalid password", "Auth", user.Id, request.Email);
                return new AuthResponse{ Message = "Invalid credentials", Code = 401 };
            }

            //Success – generate token
            var token = GenerateJwtToken(user);

            LogService.Info("Login successful", "Auth", user.Id, user.Email);

            return new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                Name = user.Name,
                Role = user.Role.ToString(),
                Code = 200,
                Message = "Login successful"
            };
        }

        public AppResponse Register(Register request)
        {
            // Check if email already exists
            if (DataStore.Users.Values.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return new AppResponse
                {
                    Code = 409,
                    Message = "User already exists.",
                    Success = false,
                    Data = null,
                    Error = null
                };
            }

            User newUser = new User
            {
                Id = DataStore.NextUserId,
                Name = request.Name ?? "N/A",
                Email = request.Email ?? "N/A",
                Role = request.Role ?? Roles.student,
                IsActive = true
            };
            newUser.SetPassword(request.Password!);

            DataStore.Users[newUser.Id] = newUser;

            return new AppResponse
            {
                Code = 201,
                Message = $"{newUser.Name} has been registered successfully.",
                Success = true,
                Data = new { newUser.Id, newUser.Name, newUser.Email, newUser.Role },
                Error = null
            };
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
