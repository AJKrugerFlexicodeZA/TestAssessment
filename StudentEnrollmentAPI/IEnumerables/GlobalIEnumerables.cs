using MIDTIER.Models;
using StudentEnrollmentAPI.Data;

namespace StudentEnrollmentAPI.IEnumerables
{
    public class GlobalIEnumerables
    {
        public static IEnumerable<UserList> GetUserDetails() => (
            from u in DataStore.Users.Values
            join updater in DataStore.Users.Values
                on u.UpdateUserId equals updater.Id into updaterGroup
            from updater in updaterGroup.DefaultIfEmpty()
            orderby u.Id ascending
            select new UserList
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                UpdateUserId = u.UpdateUserId,
                UpdateUserName = updater?.Name ?? "System",
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }
        );

        public static IEnumerable<UserList> GetUserByIdDetails(int id) => (
            from u in DataStore.Users.Values
            join updater in DataStore.Users.Values
                on u.UpdateUserId equals updater.Id into updaterGroup
            from updater in updaterGroup.DefaultIfEmpty()
            where u.Id == id
            orderby u.Id ascending
            select new UserList
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                UpdateUserId = u.UpdateUserId,
                UpdateUserName = updater?.Name ?? "System",
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }
        );

    }
}
