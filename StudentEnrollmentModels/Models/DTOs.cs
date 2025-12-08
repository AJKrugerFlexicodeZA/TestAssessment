using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDTIER.Models
{
    public class DTOs
    {
        public class CourseDto {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public bool IsActive { get; set; }
            public bool IsEnrolled { get; set; }
        };

        public class EnrolledCourseDto {
            public int CourseId { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public int TotalStudents { get; set; }
        };

        public class EnrollmentDto {
            public int UserId { get; set; }
            public string? UserName { get; set; }
            public int CourseId { get; set; }
            public string? CourseTitle { get; set; }
            public DateTime EnrolledAt { get; set; }
        };

        public class UserEnrollmentDto
        {
            public int UserId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public Roles Role { get; set; }
            public DateTime EnrolledAt { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
