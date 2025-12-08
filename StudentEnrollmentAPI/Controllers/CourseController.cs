// Controllers/CoursesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentEnrollmentAPI.Data;
using StudentEnrollmentAPI.Services;
using MIDTIER.Models;
using System.Security.Claims;
using static MIDTIER.Models.DTOs;
using System.Threading.Tasks;

namespace StudentEnrollmentAPI.Controllers;
[Authorize]
[Route("api/courses")]
[ApiController]
public class CoursesController : ControllerBase
{
    private readonly CourseService _courseService;
    private readonly EnrollmentService _enrollmentService;

    public CoursesController(CourseService courseService, EnrollmentService enrollmentService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
    }

    private int userId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("available")]
    public ActionResult<AppResponse> GetAvailable()
    {   
        AppResponse response = _courseService.GetAll(userId);
        //return StatusCode(response.Code, response); this will return request based of status code.
        return Ok(response);
    }

    [HttpGet("enrolled/{user:int}")]
    public ActionResult GetEnrolled(int user)
    {
        AppResponse<List<Enrollment.EnrolledCourses>> response = _enrollmentService.GetEnrolledCourses(user,userId);
        //return StatusCode(response.Code, response); this will return request based of status code.
        return Ok(response);
    }   

    [HttpPost("enroll/{courseId:int}-{user:int}")]
    public IActionResult Enroll(int courseId,int user)
    {
        AppResponse response = _enrollmentService.EnrollUser(user, courseId, userId);
        //return StatusCode(response.Code, response);
        return Ok(response);
    }

    [HttpDelete("deregister/{courseId:int}-{user:int}")]
    public IActionResult Deregister(int courseId, int user)
    {
        AppResponse response = _enrollmentService.UnenrollUser(user, courseId, userId);
        //return StatusCode(response.Code, response);
        return Ok(response);
    }

    [Authorize(Roles = "admin")]
    [HttpGet]
    public ActionResult<IReadOnlyList<Course>> GetAll() {
        AppResponse response = _courseService.GetAll(userId);
        return Ok(response);
        //return StatusCode(response.Code, response);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult Create([FromBody] Course course)
    {
        AppResponse response = _courseService.Add(course,userId);
        return Ok(response);
        //return StatusCode(response.Code, response);
    }

    [Authorize(Roles = "admin")]
    [HttpPut]
    public IActionResult Update([FromBody] Course course)
    {
        AppResponse response = _courseService.Update(course, userId);
        return Ok(response);
        //return StatusCode(response.Code, response);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public ActionResult<AppResponse> Delete(int id)
    {
        AppResponse response = _courseService.Delete(id, userId);
        return Ok(response);
        //return StatusCode(response.Code,response);      
    }

    [Authorize(Roles = "admin")]
    [HttpPost("exclude-users/{id:int}")]
    public ActionResult<AppResponse> ExcludeUsers(int id, [FromBody] List<int> userIds)
    {
        AppResponse response = _enrollmentService.BulkUserCourseRemoval(userIds,id, userId);
        return Ok(response);
        //return StatusCode(response.Code, response);
    }

    [Authorize(Roles = "admin")]
    [HttpGet("enrolled-users/{courseId:int}")]
    public ActionResult<AppResponse<List<EnrolledUsers>>> GetUsersEnrolledByCourseId(int courseId)
    {
        AppResponse<List<EnrolledUsers>> response = _enrollmentService.GetUsersEnrolledByCourseId(courseId, userId);
        return Ok(response);
        //return StatusCode(response.Code, response);
    }
}