// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MIDTIER.Models;
using StudentEnrollmentAPI.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using StudentCourseEnrollments.Services.StudentCourseEnrollments.Services;
using StudentEnrollmentAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace userEnrollmentAPI.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("validate")]
    [Authorize]
    public IActionResult Validate()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User ID missing from token." });

        if (!int.TryParse(userId, out var user))
            return Unauthorized(new { message = "Invalid user ID format." });

        if (!DataStore.Users.TryGetValue(user, out var userData))
            return Unauthorized(new { message = "User not found." });

        if (!userData.IsActive)
            return Unauthorized(new { message = "User inactive or deleted." });

        return Ok(new
        {
            Id = userData.Id.ToString(),
            Name = userData.Name,
            Role = userData.Role.ToString(),
            IsActive = userData.IsActive
        });
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] Register request)
    {
        var response = _authService.Register(request);
        //return StatusCode(response.Code, response);
        return Ok(response);
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] Login request)
    {
        var response = _authService.Login(request);
        //return StatusCode(response.Code, response);
        return Ok(response);
    }


}
