using System.Security.Claims;
using MIDTIER.Models;
using StudentCourseEnrollments.Services.StudentCourseEnrollments.Services;
using StudentEnrollmentAPI.Data;
using StudentEnrollmentAPI.IEnumerables;
using StudentEnrollmentAPI.Interfaces;

namespace StudentEnrollmentAPI.Services
{
    public class UserService : IUserService
    {
        public AppResponse<List<UserList>> GetAll(int currentUserId)
        {
            if (!DataStore.Users.TryGetValue(currentUserId, out var currentUser))
            {
                LogService.Warn($"User ID {currentUserId} not found during GetAll", "Users", currentUserId);
                return new AppResponse<List<UserList>>
                {
                    Code = 404,
                    Message = "User not found",
                    Success = false
                };
            }

            List<UserList> users;

            switch (currentUser.Role)
            {
                case Roles.admin:
                    // Admin sees EVERYONE
                    users = GlobalIEnumerables.GetUserDetails().ToList();
                    break;

                case Roles.instructor:
                    // Instructor sees only their students
                    users = GlobalIEnumerables.GetUserDetails()
                        .Where(u => u.Role == Roles.student)
                        .ToList();
                    break;

                case Roles.student:
                    // Student sees only their instructors
                    users = GlobalIEnumerables.GetUserDetails()
                        .Where(u => u.Role == Roles.instructor)
                        .ToList();
                    break;

                default:
                    users = new List<UserList>();
                    break;
            }

            LogService.Info($"User {currentUserId} ({currentUser.Role}) retrieved {users.Count} records", "Users", currentUserId);

            if (users.Count > 0)
            {
                return new AppResponse<List<UserList>>
                {
                    Code = 200,
                    Success = true,
                    Message = $"{users.Count} users found",
                    Data = users
                };
            }

            return new AppResponse<List<UserList>>
            {
                Code = 404,
                Success = false,
                Message = "No users found for your role",
                Data = new()
            };
        }
        public AppResponse GetById(int id,int updateUserId)
        {
            //Not Zero
            if(id == 0)
            {
                LogService.Warn($"User ID can not be 0", "Users", updateUserId);
                return new AppResponse
                {
                    Code = 400,
                    Message = "Invalid User Id",
                    Success = false,
                    Data = null,
                    Error = "User Id cannot be zero",
                };
            }
                
            User? user = DataStore.Users.TryGetValue(id, out var User) ? User : null;
            //Not Found
            if (user == null)
            {
                LogService.Error($"User does not exist", "Users",updateUserId);
                return new AppResponse
                {
                    Code = 404,
                    Message = "User not found",
                    Success = false,
                    Data = null,
                    Error = "No User available with the given Id",
                };
            }
            LogService.Info($"User {id} retrieved record", "Users", updateUserId);
            return new AppResponse
            {
                Code = 200,
                Message = "User Record Found",
                Success = true,
                Data = user,
                Error = null
            };
        }

        public AppResponse Add(User user,int updateUserId)
        {
            //Check for invalid data
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                LogService.Error($"User attempted to add incorret data", "Users", updateUserId);
                return new AppResponse
                {
                    Code = 400, // Bad Request
                    Message = "Invalid user data",
                    Success = false,
                    Data = null,
                    Error = "Email is required"
                };
            }

            //Remove trailing whitespaces
            user.Email = user.Email.Trim().ToLowerInvariant();

            //Checks if user exist via email
            bool emailExists = DataStore.Users.Values
                .Any(u => string.Equals(u.Email, user.Email, StringComparison.OrdinalIgnoreCase));

            if (emailExists)
            {
                LogService.Error($"Error 2627 : User already exists", "Users", updateUserId);
                return new AppResponse
                {
                    Code = 409,
                    Message = "User already exists",
                    Success = false,
                    Data = null,
                    Error = "A user with this email address already exists"
                };
            }
                
            //Sets New ID
            user.Id = DataStore.NextUserId;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdateUserId = updateUserId;
            //Hashes Password
            user.SetPassword(user.Password!);
            //Stores user
            DataStore.Users[user.Id] = user;
            LogService.Info($"Record Created Successfullly", "Users", updateUserId);
            return new AppResponse
            {
                Code = 201,
                Message = "User created successfully",
                Success = true,
                Data = user,
                Error = null
            };
        }

        public AppResponse Update(User user,int updateUserId)
        {   //Conditional Check If User is Null
            if (user == null)
            {
                LogService.Info($"InvalidUserData", "Users", updateUserId);
                //Return BadRequest 400 Because User is null
                return new AppResponse
                {
                    Code = 400,
                    Message = "Invalid User Data",
                    Success = false,
                    Data = null,
                    Error = "User data cannot be null",
                };
            }
                
            //Conditional Check If User Exists
            if (!DataStore.Users.ContainsKey(user.Id))
            {
                LogService.Error($"User not found", "Users", updateUserId);
                return new AppResponse
                {
                    Code = 404,
                    Message = "User not found",
                    Success = false,
                    Data = null,
                    Error = "No User available with the given Id",
                };
            }
                
            //Update User
            user.UpdateUserId = updateUserId;
            user.UpdatedAt = DateTime.UtcNow;
            DataStore.Users[user.Id] = user;
            LogService.Error($"User [{user.Id}] updated", "Users", updateUserId);
            return new AppResponse
            {
                Code = 200,
                Message = "User Updated Successfully",
                Success = true,
                Data = user,
                Error = null
            };
        }

        public AppResponse Delete(int id, int updateUserId)
        {   //Check if UserId is Zero
            if(id == 0)
            {
                LogService.Error($"Invalid User Id", "Users", updateUserId);
                return new AppResponse
                {
                    Code = 400,
                    Message = "Invalid User Id",
                    Success = false,
                    Data = null,
                    Error = "User Id cannot be zero",
                };
            }
                
            //User Id Not Zero -> Disable User
            DataStore.Users[id].UpdateUserId = updateUserId;
            DataStore.Users[id].UpdatedAt = DateTime.UtcNow;
            DataStore.Users[id].IsActive = false;
            LogService.Info($"User [{id}] Deactivated", "Users", updateUserId);
            return new AppResponse {
                Code = 200,
                Message = "User Deactivated Successfully",
                Success = true,
                Data = null,
                Error = null
            };
        }

        public AppResponse ToggleActive(int id,int updateUserId)
        {   //Check if UserId is Zero
            if (id == 0)
            {
                LogService.Error($"Invalid UserId", "Users", updateUserId);
                return new AppResponse
                {
                    Code = 400,
                    Message = "Invalid User Id",
                    Success = false,
                    Data = null,
                    Error = "User Id cannot be zero",
                };
            }
                
            //Toggle Active Status
            DataStore.Users[id].UpdateUserId = updateUserId;
            DataStore.Users[id].UpdatedAt = DateTime.UtcNow;
            DataStore.Users[id].IsActive = !DataStore.Users[id].IsActive;
            LogService.Info($"User [{id}] IsActive Status Toggled From {!DataStore.Users[id].IsActive} to {DataStore.Users[id].IsActive}", "Users", updateUserId);
            return new AppResponse
            {
                Code = 200,
                Message = "User Active Status Toggled Successfully",
                Success = true,
                Data = DataStore.Users[id],
                Error = null
            };
        }
    }
}
