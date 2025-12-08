using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentEnrollmentAPI.Data;
using StudentEnrollmentAPI.Services;
using MIDTIER.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using StudentEnrollmentAPI.Interfaces;
using System.Security.Claims;

namespace StudentEnrollmentAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }
        private int userId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [Authorize(Roles = "admin")]
        [HttpGet]
        public ActionResult<AppResponse<List<UserList>>> GetAllUsers()
        {
            AppResponse<List<UserList>> response = _userService.GetAll(userId);
            return StatusCode(response.Code, response);
        }

        [Authorize(Roles ="admin")]
        [HttpPost]
        public IActionResult CreateUser([FromBody] User user)
        {
            AppResponse response = _userService.Add(user,userId);
            return StatusCode(response.Code, response);
        }

        [Authorize(Roles = "admin,student,instructor")]
        [HttpGet("{id:int}")]
        public ActionResult<User> GetUserById(int id)
        {
            AppResponse response = _userService.GetById(id, userId);
            return StatusCode(response.Code, response);
        }

        [Authorize(Roles = "admin,student")]
        [HttpPut]
        public IActionResult UpdateUser([FromBody] User user)
        {
            AppResponse response = _userService.Update(user, userId);
            return StatusCode(response.Code, response);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public IActionResult DeleteUser(int id)
        {
            AppResponse response = _userService.Delete(id,userId);
            return StatusCode(response.Code, response);
        }

        [Authorize(Roles = "admin")]
        [HttpPatch("{id:int}/toggle-active")]
        public IActionResult ToggleUserActive(int id)
        {
            AppResponse response = _userService.ToggleActive(id, userId);
            return StatusCode(response.Code, response);
        }
        
    }

}

