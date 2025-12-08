using Microsoft.EntityFrameworkCore;
using MIDTIER.Models;
using StudentEnrollmentAPI.Data;

namespace StudentEnrollmentAPI.Helper
{
    public class UserHelper
    {
        public Task<string> GetRoleByUserIdAsync(int userId)
        {
            if (DataStore.Users.TryGetValue(userId, out var user))
            {
                return Task.FromResult(user.Role.ToString());
            }

            throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        // Bonus: Get full user
        public Task<User?> GetUserByIdAsync(int userId)
        {
            DataStore.Users.TryGetValue(userId, out var user);
            return Task.FromResult(user);
        }

        // Bonus: Check if user exists
        public Task<bool> UserExistsAsync(int userId)
            => Task.FromResult(DataStore.Users.ContainsKey(userId));
    }
}
