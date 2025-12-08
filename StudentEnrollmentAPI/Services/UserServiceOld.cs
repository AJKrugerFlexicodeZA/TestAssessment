using Microsoft.EntityFrameworkCore;
using MIDTIER.Models;
using StudentEnrollmentAPI.Data;
using StudentEnrollmentAPI.Helper;

namespace StudentEnrollmentAPI.Services
{
    public class UserServiceOld
    {
        private readonly AppDbContext _context;

        public UserServiceOld(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AppResponse<List<User>>> GetAllUsers()
        {
            List<User>? users = await _context.Users.OrderBy(x => x.name).ToListAsync();

            if(users == null)
                return new AppResponse<List<User>> { 
                code = 404,
                message = "No users found.",
                data = users
                };
            return new AppResponse<List<User>>
            {
                code = 200,
                message = "Ok",
                data = users
            };
        }

        public async Task<AppResponse<int?>> UpdateUser (int id,User user)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return new AppResponse<int?>
                {
                    code= 404,
                    message= "User not found."
                };

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();

            return new AppResponse<int?>
            {
                code = 200,
                message = "User details updated successfully"
            };
        }
        /*Not Actually Removing User, but disabling him
         Its best to rather disable a user, than to delete him with all data linkages*/
        public async Task<AppResponse<int?>> RemoveUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.ID == id);
            if (user == null)
                return new AppResponse<int?>
                {
                    code = 404,
                    message = "User not found."
                };
            user.isActive = false;
            _context.Entry(user).CurrentValues.SetValues(user);
            return new AppResponse<int?>
            {
                code = 200,
                message = "User have been disabled"
            };
        }
        //Return Single User
        public async Task<User> GetUserById(int id)
        {
            return await _context.Users.FirstOrDefaultAsync( x => x.ID == id) ?? new();
        }
    }
}
