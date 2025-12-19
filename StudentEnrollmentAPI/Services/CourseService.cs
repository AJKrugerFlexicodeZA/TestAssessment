// Services/CourseService.cs
using MIDTIER.Models;
using StudentCourseEnrollments.Services;
using StudentEnrollmentAPI.Data;
using StudentEnrollmentAPI.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StudentEnrollmentAPI.Services
{
    public class CourseService : ICourseService
    {
        public AppResponse GetAll(int updateUserId) {
           
           List<Course> data = DataStore.Courses.Values.ToList();

            if (data.Count > 0)
            {
                LogService.Info($"User retrieved [{data.Count}] records", "Courses", updateUserId);
                return new AppResponse
                {
                    Code = 200,
                    Message = "Success",
                    Success = true,
                    Data = data,
                    Error = null
                };
            }
            LogService.Info($"No records found", "Courses", updateUserId);
            return new AppResponse {
                Code = 404,
                Message = "No courses found",
                Success = false,
                Data = null,
                Error = "No courses available in the system",
            };
        }

        public AppResponse GetById(int id, int updateUserId)
        {            
            if (!DataStore.Courses.ContainsKey(id))
            {
                LogService.Info($"No records found by UserId [{id}]", "Courses", updateUserId);
                return new AppResponse
                {
                    Code = 404,
                    Message = "Course not found",
                    Success = false,
                    Data = null,
                    Error = null
                };
            }
                
            Course course = DataStore.Courses.TryGetValue(id, out var c) 
                ? c 
                : new Course();
            LogService.Info($"Record found by UserId [{id}]", "Courses", updateUserId);
            return new AppResponse {
                Code = 200,
                Message = "Success",
                Success = true,
                Data = course,
                Error = null
            };
        }

        public AppResponse Add(Course course, int updateUserId)
        {
            //Validate Title
            if (string.IsNullOrWhiteSpace(course.Title))
            {
                LogService.Warn($"User [{updateUserId}] Attempted To Create Course without Title", "Courses", updateUserId);
                return new AppResponse
                {
                    Code = 400,
                    Message = "Course title is required",
                    Success = false,
                    Data = null,
                    Error = "Title cannot be empty"
                };
            }
                
            //Remove unneccesarry white space
            course.Title = course.Title.Trim();
            //Check for duplicate titles
            bool exists = DataStore.Courses.Values
                .Any(c => string.Equals(c.Title, course.Title, StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                LogService.Warn($"User [{updateUserId}] Attempted To Create Duplicate Course Title [{course.Title}]", "Courses", updateUserId);
                //Return Conflict
                return new AppResponse
                {
                    Code = 409,
                    Message = "A course with this title already exists",
                    Success = false,
                    Data = null,
                    Error = "Duplicate course title"
                };
            }
                
            //Add Course If Not Exists
            course.Id = DataStore.NextCourseId;
            DataStore.Courses[course.Id] = course;
            LogService.Info($"User [{updateUserId}] Created Course [{course.Title}] Successfully", "Courses", updateUserId);
            return new AppResponse
            {
                Code = 201,
                Message = "Course created successfully",
                Success = true,
                Data = course
            };
        }

        public AppResponse Update(Course course, int updateUserId)
        {
            if (!DataStore.Courses.ContainsKey(course.Id)) { 
                LogService.Error($"User [{updateUserId}] Attempted To Update Non-Existent Course Id [{course.Id}]", "Courses", updateUserId);
                return new AppResponse
                {
                    Code = 404,
                    Message = "Course not found",
                    Success = false,
                    Data = null,
                    Error = null
                };
            };
            DataStore.Courses[course.Id] = course;
            LogService.Info($"User [{updateUserId}] Updated Course Id [{course.Id}] Successfully", "Courses", updateUserId);
            return new AppResponse { 
                Code = 200,
                Message = "Course Updated Successfully",
                Success = true,
                Data = course,
                Error = null
            };
        }
        public AppResponse Delete(int id,int updateUserId) 
        {
            if(id == 0)
            {
                LogService.Error($"User [{updateUserId}] Attempted To Delete Course with Invalid Id [{id}]", "Courses", updateUserId);
                return new AppResponse
                {
                    Code = 400,
                    Message = "Invalid Course Id",
                    Success = false,
                    Data = null,
                    Error = "Course Id cannot be zero",
                };
            }
                
            if (!DataStore.Courses.ContainsKey(id))
            {
                LogService.Error($"User [{updateUserId}] Attempted To Delete Non-Existent Course Id [{id}]", "Courses", updateUserId);
                return new AppResponse
                {
                    Code = 404,
                    Message = "Course not found",
                    Success = false,
                    Data = null,
                    Error = "No Course available with the given Id",
                };
            }
                
            DataStore.Courses.Remove(id);
            LogService.Info($"User [{updateUserId}] Deleted Course Id [{id}] Successfully", "Courses", updateUserId);
            return new AppResponse
            {
                Code = 200,
                Message = "Course Deleted Successfully",
                Success = true,
                Data = null,
                Error = null
            };

        }
    }
}
