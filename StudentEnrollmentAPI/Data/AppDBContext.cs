using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MIDTIER.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace StudentEnrollmentAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite key for Enrollment
            modelBuilder.Entity<Enrollment>()
                .HasKey(e => new { e.user_id, e.course_id });

            // Seed initial courses
            modelBuilder.Entity<Course>().HasData(
                new Course { id = 1, title = "C# Advanced", description = "Deep dive into C#", is_active=true },
                new Course { id = 2, title = "Blazor Mastery", description = "Build modern web apps", is_active = true },
                new Course { id = 3, title = "Entity Framework", description = "ORM with EF Core" , is_active = true }
            );

        }
    }
}
