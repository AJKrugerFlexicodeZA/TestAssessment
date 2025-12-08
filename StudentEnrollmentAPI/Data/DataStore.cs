// StudentEnrollmentAPI.Data/DataStore.cs
using Microsoft.AspNetCore.Identity;
using MIDTIER.Models;

namespace StudentEnrollmentAPI.Data;

public static class DataStore
{
    public static Dictionary<int, Course> Courses { get; } = new();
    public static Dictionary<int, User> Users { get; } = new();
    public static Dictionary<int, HashSet<int>> UserEnrolledCourses { get; set; } = new();
    public static Dictionary<(int UserId, int CourseId), DateTime> EnrollmentDates { get; set; } = new();
    public static Dictionary<int, ApplicationLogs> ApplicationLogs { get; } = new();
    static DataStore()
    {
        SeedData();
    }

    private static void SeedData()
    {
        // === Seed Courses ===
        var courses = new []
        {
            new Course{Id = 1, Title = "C# Advanced", Description = "Deep dive into C#", IsActive = true},
            new Course{Id = 2, Title = "Blazor Mastery", Description = "Build SPAs with Blazor", IsActive = true},
            new Course{Id = 3, Title = "Clean Architecture", Description = "Scalable .NET patterns", IsActive = true},
            new Course{Id = 4, Title = "Minimal APIs", Description =  "Build APIs in 10 lines", IsActive = true},
            new Course{Id = 5, Title= "Performance .NET", Description =  "Zero allocations & unsafe code", IsActive = true}
        };

        foreach (var c in courses)
            Courses[c.Id] = c;

        // === Seed Users with Secure Passwords ===
        var hasher = new PasswordHasher<User>();

        User admin = new User {
            Id = 1,
            Name = "admin",
            Email = "admin@gmail.com",
            Role = Roles.admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdateUserId = 1
        };
        admin.SetPassword("admin");

        User admin1 = new User {
            Id = 2,
            Name = "admin1",
            Email = "admin1@gmail.com",
            Role = Roles.admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdateUserId = 1
        };
        admin1.SetPassword("admin1");

        User student = new User {
            Id = 3,
            Name = "student",
            Email = "student@gmail.com",
            Role = Roles.student,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdateUserId = 1
        };
        student.SetPassword("student");

        User instructor = new User {
            Id = 4,
            Name = "teacher",
            Email = "teacher@example.com",
            Role = Roles.instructor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdateUserId = 1
        };
        instructor.SetPassword("teacher");

        Users[1] = admin;
        Users[2] = admin1;
        Users[3] = student;
        Users[4] = instructor;

        // === Seed Enrollments + Dates (CORRECT WAY) ===
        var now = DateTime.UtcNow;

        // Admin1 enrolled in C# Advanced, Clean Arch, Performance .NET
        UserEnrolledCourses[2] = new HashSet<int> { 1, 3, 5 };
        EnrollmentDates[(2, 1)] = now.AddDays(-30);
        EnrollmentDates[(2, 3)] = now.AddDays(-25);
        EnrollmentDates[(2, 5)] = now.AddDays(-10);

        // Student enrolled in Blazor Mastery and Minimal APIs
        UserEnrolledCourses[3] = new HashSet<int> { 2, 4 };
        EnrollmentDates[(3, 2)] = now.AddDays(-20);
        EnrollmentDates[(3, 4)] = now.AddDays(-15);

        // Optional: Let teacher be enrolled in one course too
        UserEnrolledCourses[4] = new HashSet<int> { 1 };
        EnrollmentDates[(4, 1)] = now.AddDays(-40);
    }

    // Perfect, clean helpers
    public static int NextCourseId => Courses.Keys.DefaultIfEmpty(0).Max() + 1;
    public static int NextUserId => Users.Keys.DefaultIfEmpty(0).Max() + 1;
    public static int NextLogId => ApplicationLogs.Keys.DefaultIfEmpty(0).Max() + 1;
}