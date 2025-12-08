// Controllers/ApplicationLogController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIDTIER.Models;
using StudentCourseEnrollments.Services;
using StudentCourseEnrollments.Services.StudentCourseEnrollments.Services;
using StudentEnrollmentAPI.Data;

namespace StudentEnrollmentAPI.Controllers
{
    [Authorize(Roles = "admin")]  // Only admins can view or clear logs
    [ApiController]
    [Route("api/logs")]
    public class ApplicationLogController : ControllerBase
    {
        // GET: api/logs
        // Returns all logs, newest first
        [HttpGet]
        public IActionResult GetLogs()
        {
            try
            {
                var logs = DataStore.ApplicationLogs
                    .OrderByDescending(l => l.Value.CreatedAt)
                    .Select(l => new
                    {
                        l.Value.Id,
                        l.Value.Message,
                        CreatedAt = l.Value.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        l.Value.TableName,
                        Level = l.Value.Level?.ToString() ?? "Info",
                        UserId = l.Value.UserId ?? 0,
                        UserName = l.Value.UserId.HasValue && DataStore.Users.TryGetValue(l.Value.UserId.Value, out var user)
                            ? user.Name
                            : "System"
                    })
                    .ToList();

                return Ok(new AppResponse<object>
                {
                    Code = 200,
                    Success = true,
                    Message = $"Retrieved {logs.Count} log entries",
                    Data = logs
                });
            }
            catch (Exception ex)
            {
                LogService.Error("Failed to retrieve logs", "Logs", 0, ex.Message);
                return StatusCode(500, new AppResponse
                {
                    Code = 500,
                    Success = false,
                    Message = "Internal server error",
                    Error = ex.Message
                });
            }
        }

        // DELETE: api/logs
        // Clears ALL logs — dangerous, so admin + confirmation
        [HttpDelete]
        public IActionResult ClearAllLogs()
        {
            try
            {
                int count = DataStore.ApplicationLogs.Count;

                if (count == 0)
                {
                    return Ok(new AppResponse
                    {
                        Code = 200,
                        Success = true,
                        Message = "No logs to delete"
                    });
                }

                DataStore.ApplicationLogs.Clear();

                // Log the action
                LogService.Info($"All {count} logs cleared by admin", "Logs", GetCurrentUserId());

                return Ok(new AppResponse
                {
                    Code = 200,
                    Success = true,
                    Message = $"Successfully deleted {count} log entries"
                });
            }
            catch (Exception ex)
            {
                LogService.Critical("Failed to clear logs", "Logs", GetCurrentUserId(), ex.Message);
                return StatusCode(500, new AppResponse
                {
                    Code = 500,
                    Success = false,
                    Message = "Failed to clear logs",
                    Error = ex.Message
                });
            }
        }

        // Optional: GET by ID
        [HttpGet("{id:int}")]
        public IActionResult GetLog(int id)
        {
            var log = DataStore.ApplicationLogs.FirstOrDefault(l => l.Value.Id == id);
            if (log.Value == null)
                return NotFound(new AppResponse { Code = 404, Message = "Log not found" });

            return Ok(new AppResponse<object>
            {
                Code = 200,
                Success = true,
                Data = new
                {
                    log.Value.Id,
                    log.Value.Message,
                    log.Value.CreatedAt,
                    log.Value.TableName,
                    log.Value.Level,
                    log.Value.UserId
                }
            });
        }

        // Helper: Get current user ID from JWT (if available)
        private int GetCurrentUserId()
        {
            if (int.TryParse(User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value, out int userId))
                return userId;
            return 0; // System if not found
        }
    }
}