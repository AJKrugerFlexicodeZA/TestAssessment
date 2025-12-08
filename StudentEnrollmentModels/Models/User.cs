using Microsoft.AspNetCore.Identity;

namespace MIDTIER.Models
{
    public enum Roles { admin, student, instructor }

    public class UserList
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Roles Role { get; set; }
        public bool IsActive { get; set; }
        public int? UpdateUserId { get; set; }
        public string? UpdateUserName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Roles Role { get; set; }
        public bool IsActive { get; set; }
        public string PasswordHash { get; private set; } = string.Empty;
        public string? Password { get; set; }
        public int? UpdateUserId { get; set; }
        public string? UpdateUserName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public void SetPassword(string plainPassword)
        {
            var hasher = new PasswordHasher<User>();
            PasswordHash = hasher.HashPassword(this, plainPassword);
        }

        public bool VerifyPassword(string plainPassword)
        {
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(this, PasswordHash, plainPassword);
            return result != PasswordVerificationResult.Failed;
        }
        public User ToSafeUser()
        {
            return new User
            {
                Id = Id,
                Name = Name,
                Email = Email,
                Role = Role,
                IsActive = IsActive,
                PasswordHash = "[HIDDEN]"
            };
        }
    }
}