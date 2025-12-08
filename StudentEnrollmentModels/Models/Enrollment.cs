using System.ComponentModel.DataAnnotations.Schema;

namespace MIDTIER.Models
{
    public class Enrollment
    {
        public int UserId { get;set; }
        public int CourseId { get; set; }
        public DateTime EnrolledAt { get; set; }
        public User? User { get;set; }
        public Course? Course { get;set; }
        public class EnrolledCourses {
            public int CourseId { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public int TotalUsers { get; set; }
            public string? InstructorName { get; set; }
            public int? InstructorId { get; set; }
            public HashSet<int>? UserIds { get; set; }
        };
    }
    public class EnrolledUsers
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
        public bool IsActive { get; set; }
    }
}
