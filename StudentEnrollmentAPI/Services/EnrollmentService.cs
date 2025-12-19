// Services/EnrollmentService.cs
using StudentEnrollmentAPI.Data;
using StudentEnrollmentAPI.Interfaces;
using MIDTIER.Models;
using static MIDTIER.Models.DTOs;
using System.Collections.Generic;
using StudentCourseEnrollments.Services;

namespace StudentEnrollmentAPI.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private static readonly Dictionary<int, int> _courseEnrollmentCount = new();

        private static void UpdateCache(int courseId, int delta)
        {
            if (_courseEnrollmentCount.TryGetValue(courseId, out var count))
            {
                int newCount = count + delta;
                if (newCount <= 0)
                    _courseEnrollmentCount.Remove(courseId);
                else
                    _courseEnrollmentCount[courseId] = newCount;
            }
            else if (delta > 0)
            {
                _courseEnrollmentCount[courseId] = delta;
            }
        }

        public AppResponse<List<Enrollment>> GetAllEnrollments(int updateUserId)
        {
            List<Enrollment> list = new();
            foreach (var kvp in DataStore.UserEnrolledCourses)
            {
                int userId = kvp.Key;
                var courseIds = kvp.Value;
                var user = DataStore.Users.GetValueOrDefault(userId);
                foreach (var courseId in courseIds)
                {
                    var course = DataStore.Courses.GetValueOrDefault(courseId);
                    DataStore.EnrollmentDates.TryGetValue((userId, courseId), out var enrolledAt);
                    list.Add(new Enrollment
                    {
                        EnrolledAt = enrolledAt != default ? enrolledAt : DateTime.UtcNow,
                        User = user,
                        Course = course,
                        UserId = userId,
                        CourseId = courseId
                    });
                }
            }
            if (list.Count > 0)
            {
                LogService.Info($"User retrieved [{list.Count}] records", "Enrollments", updateUserId);
                return new AppResponse<List<Enrollment>>
                {
                    Code = 200,
                    Message = "Success",
                    Success = true,
                    Data = list,
                    Error = null
                };
            }
            LogService.Info($"User retrieved [{list.Count}] records", "Enrollments", updateUserId);
            return new AppResponse<List<Enrollment>>
            {
                Code = 404,
                Message = "No enrollments found",
                Success = false,
                Data = null,
                Error = "No enrollments available in the system"
            };
        }

        public AppResponse GetEnrollmentsByUserId(int userId, int updateUserId)
        {
            if (!DataStore.UserEnrolledCourses.TryGetValue(userId, out var courseIds))
            {
                LogService.Info($"User retrieved [0] records for UserId [{userId}]", "Enrollments", updateUserId);
                return new AppResponse
                {
                    Code = 404,
                    Message = "No enrollments found for the user",
                    Success = false,
                    Data = null,
                    Error = null
                };
            }
            var result = new List<Enrollment>(courseIds.Count);
            foreach (var courseId in courseIds)
            {
                var course = DataStore.Courses.GetValueOrDefault(courseId);
                DataStore.EnrollmentDates.TryGetValue((userId, courseId), out var enrolledAt);
                result.Add(new Enrollment
                {
                    UserId = userId,
                    CourseId = courseId,
                    EnrolledAt = enrolledAt != default ? enrolledAt : DateTime.UtcNow,
                    Course = course
                });
            }
            LogService.Info($"User retrieved [{result.Count}] records for UserId [{userId}]", "Enrollments", updateUserId);
            return new AppResponse
            {
                Code = 200,
                Message = "Success",
                Success = true,
                Data = result,
                Error = null
            };
        }

        public AppResponse<List<Enrollment.EnrolledCourses>> GetEnrolledCourses(int userId, int updateUserId)
        {
            // Validate the requesting user exists
            if (!DataStore.Users.TryGetValue(updateUserId, out var currentUser))
            {
                LogService.Warn($"Requesting user {updateUserId} not found", "Enrollments", updateUserId);
                return new AppResponse<List<Enrollment.EnrolledCourses>>
                {
                    Code = 404,
                    Success = false,
                    Message = "User not found"
                };
            }
            LogService.Info($"GetEnrolledCourses called by {currentUser.Role} (ID: {updateUserId})", "Enrollments", updateUserId);
            //Admin sees all courses, Student sees only their enrolled ones
            var result = (
                from course in DataStore.Courses.Values
                where course.IsActive
                //Count how many students are enrolled in this course
                let totalStudents = DataStore.UserEnrolledCourses.Count(e => e.Value.Contains(course.Id))
                //Check if current user (student) is enrolled in this course
                let isEnrolled = currentUser.Role == Roles.student &&
                                 DataStore.UserEnrolledCourses.TryGetValue(updateUserId, out var userCourses) &&
                                 userCourses.Contains(course.Id)
                //ROLE FILTER: Admin sees all, Student sees only enrolled
                where currentUser.Role == Roles.admin || isEnrolled
                orderby course.Title
                select new Enrollment.EnrolledCourses
                {
                    CourseId = course.Id,
                    Title = course.Title ?? "Unknown Course",
                    Description = course.Description ?? "No description available",
                    TotalUsers = totalStudents
                }
            ).ToList();
            if (result.Count > 0)
            {
                LogService.Info($"Retrieved {result.Count} course(s) for {currentUser.Role} (ID: {updateUserId})",
                    "Enrollments", updateUserId);
                return new AppResponse<List<Enrollment.EnrolledCourses>>
                {
                    Code = 200,
                    Success = true,
                    Message = "Success",
                    Data = result
                };
            }
            LogService.Info($"No courses found for {currentUser.Role} (ID: {updateUserId})",
                "Enrollments", updateUserId);
            return new AppResponse<List<Enrollment.EnrolledCourses>>
            {
                Code = 404,
                Success = false,
                Message = "No courses found",
                Data = new()
            };
        }

        public AppResponse EnrollUser(int userId, int courseId, int updateUserId)
        {
            if (!DataStore.Users.ContainsKey(userId) || !DataStore.Courses.ContainsKey(courseId))
            {
                LogService.Error($"Error 404 : User [{userId}] or Course [{courseId}] not found", "Enrollments", updateUserId);
                return new AppResponse
                {
                    Code = 404,
                    Message = "User or Course not found",
                    Success = false,
                    Data = null,
                    Error = "Either the user or the course does not exist"
                };
            }
            if (!DataStore.UserEnrolledCourses.TryGetValue(userId, out var set))
            {
                set = new HashSet<int>();
                DataStore.UserEnrolledCourses[userId] = set;
            }
            if (set.Add(courseId))
            {
                DataStore.EnrollmentDates[(userId, courseId)] = DateTime.UtcNow;
                UpdateCache(courseId, +1);
                LogService.Info($"User [{userId}] enrolled in Course [{courseId}]", "Enrollments", updateUserId);
                return new AppResponse
                {
                    Code = 201,
                    Message = "Enrolled successfully",
                    Success = true,
                    Data = null,
                    Error = null
                };
            }
            LogService.Error($"Error 409 : User [{userId}] already enrolled in Course [{courseId}]", "Enrollments", updateUserId);
            return new AppResponse
            {
                Code = 409,
                Message = "Already enrolled",
                Success = false,
                Data = null,
                Error = "The user is already enrolled in the course"
            };
        }

        public AppResponse BulkUserCourseRemoval(List<int> userIds, int courseId, int updateUserId)
        {
            foreach (var userId in userIds)
            {
                UnenrollUser(userId, courseId, updateUserId);
            }
            return new AppResponse
            {
                Code = 200,
                Message = "Bulk unenrollment completed",
                Success = true,
                Data = null,
                Error = null
            };
        }

        public AppResponse UnenrollUser(int userIdToRemove, int courseId, int requestingUserId)
        {
            //Validate requesting user
            if (!DataStore.Users.TryGetValue(requestingUserId, out var requestingUser))
            {
                LogService.Warn($"Requesting user {requestingUserId} not found", "Enrollments", requestingUserId);
                return new AppResponse
                {
                    Code = 404,
                    Success = false,
                    Message = "Requesting user not found"
                };
            }
            //Only students can unenroll themselves and admins can unroll anyone
            if (requestingUser.Role == Roles.student && userIdToRemove != requestingUserId)
            {
                LogService.Warn($"Student {requestingUserId} tried to unenroll another user {userIdToRemove}",
                    "Enrollments", requestingUserId);
                return new AppResponse
                {
                    Code = 403,
                    Success = false,
                    Message = "Forbidden",
                    Error = "Students can only unenroll themselves"
                };
            }

            if (!DataStore.UserEnrolledCourses.TryGetValue(userIdToRemove, out var enrolledCourses) ||
                !enrolledCourses.Remove(courseId))
            {
                LogService.Warn($"User {userIdToRemove} is not enrolled in course {courseId}",
                    "Enrollments", requestingUserId);
                return new AppResponse
                {
                    Code = 404,
                    Success = false,
                    Message = "Not enrolled",
                    Error = "The user is not enrolled in this course"
                };
            }
            //Remove enrollment date
            DataStore.EnrollmentDates.Remove((userIdToRemove, courseId));

            //If user has no more courses → remove their entry completely
            if (enrolledCourses.Count == 0)
            {
                DataStore.UserEnrolledCourses.Remove(userIdToRemove);
                LogService.Info($"User {userIdToRemove} has no more enrollments — entry removed",
                    "Enrollments", requestingUserId);
            }
            //Update cache (student count per course)
            UpdateCache(courseId, -1);
            //Log success with context
            string actionBy = requestingUser.Role == Roles.admin
                ? $"Admin {requestingUser.Name}"
                : "Self";
            LogService.Info($"User {userIdToRemove} unenrolled from course {courseId} by {actionBy}",
                "Enrollments", requestingUserId);
            return new AppResponse
            {
                Code = 200,
                Success = true,
                Message = "Unenrolled successfully"
            };
        }

        public AppResponse IsEnrolled(int userId, int courseId, int updateUserId)
        {
            if (DataStore.UserEnrolledCourses.TryGetValue(userId, out var set) && set.Contains(courseId))
            {
                LogService.Info($"User [{userId}] is enrolled in Course [{courseId}]", "Enrollments", updateUserId);
                return new AppResponse
                {
                    Code = 200,
                    Message = "User is enrolled in the course",
                    Success = true,
                    Data = true,
                    Error = null
                };
            }
            LogService.Error($"User [{userId}] is not enrolled in Course [{courseId}]", "Enrollments", updateUserId);
            return new AppResponse
            {
                Code = 404,
                Message = "User is not enrolled in the course",
                Success = false,
                Data = false,
                Error = null
            };
        }

        public AppResponse<List<EnrolledUsers>> GetUsersEnrolledByCourseId(int courseId, int updateUserId)
        {
            try
            {
                // Validate course exists
                if (!DataStore.Courses.ContainsKey(courseId))
                {
                    return new AppResponse<List<EnrolledUsers>>
                    {
                        Success = false,
                        Code = 404,
                        Message = $"Course with ID {courseId} not found."
                    };
                }

                var enrolledUsers = DataStore.UserEnrolledCourses
                    .Where(kvp => kvp.Value.Contains(courseId))
                    .Select(kvp =>
                    {
                        int userId = kvp.Key;
                        var user = DataStore.Users.GetValueOrDefault(userId);

                        DataStore.EnrollmentDates.TryGetValue((userId, courseId), out var enrolledAt);
                        if (enrolledAt == default) enrolledAt = DateTime.UtcNow;

                        return new EnrolledUsers
                        {
                            UserId = userId,
                            Name = user?.Name ?? "Unknown User",
                            Email = user?.Email ?? "N/A",
                            EnrolledAt = enrolledAt,
                            IsActive = user?.IsActive ?? false
                        };
                    })
                    .OrderBy(u => u.Name)
                    .ToList();

                return new AppResponse<List<EnrolledUsers>>
                {
                    Success = true,
                    Code = 200,
                    Message = enrolledUsers.Any()
                        ? $"{enrolledUsers.Count} enrolled user(s) found."
                        : "No users enrolled in this course.",
                    Data = enrolledUsers
                };
            }
            catch (Exception ex)
            {
                LogService.Error($"Error fetching enrolled users for course {courseId}", ex.Message, updateUserId);
                return new AppResponse<List<EnrolledUsers>>
                {
                    Success = false,
                    Code = 500,
                    Message = "Server error"
                };
            }
        }

        public AppResponse GetEnrollmentCountForCourse(int courseId, int updateUserId)
        {
            if (courseId == 0)
            {
                LogService.Error($"Invalid Course Id [{courseId}]", "Enrollments", updateUserId);
                return new AppResponse
                {
                    Code = 400,
                    Message = "Invalid Course Id",
                    Success = false,
                    Data = 0,
                    Error = "Course Id cannot be zero"
                };
            }
            var count = _courseEnrollmentCount.GetValueOrDefault(courseId);
            if (count == 0)
            {
                LogService.Info($"No enrollments found for Course Id [{courseId}]", "Enrollments", updateUserId);
                return new AppResponse
                {
                    Code = 404,
                    Message = "No enrollments found for the course",
                    Success = false,
                    Data = 0,
                    Error = null
                };
            }
            LogService.Info($"Enrollment count for Course Id [{courseId}] is [{count}]", "Enrollments", updateUserId);
            return new AppResponse
            {
                Code = 200,
                Message = "Success",
                Success = true,
                Data = count,
                Error = null
            };
        }
    }
}