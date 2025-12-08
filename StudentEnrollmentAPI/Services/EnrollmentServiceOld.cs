using StudentEnrollmentAPI.Data;
using Microsoft.EntityFrameworkCore;
using MIDTIER.Models;
using Microsoft.SqlServer;
using StudentEnrollmentAPI.Helper;
namespace StudentEnrollmentAPI.Services
{
    public class EnrollmentServiceOld
    {
        private readonly AppDbContext _context;
        private readonly UserHelper _userHelper;
    
        public EnrollmentServiceOld(AppDbContext context, UserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task<AppResponse<Course[]>> GetAvailableCoursesAsync(int UID)
        {
            string role = await _userHelper.GetRoleByUserIdAsync(UID);

            Course[] courses;

            if (role == "admin")
            {
                courses = await (from c in _context.Courses.AsNoTracking()
                                 orderby c.title
                                 select c).ToArrayAsync();
            }
            else
            {
                courses = await (from c in _context.Courses.AsNoTracking()
                                 join e in _context.Enrollments.AsNoTracking()
                                 on c.ID equals e.courseID
                                 where e.userID == UID
                                 orderby c.title
                                 select c)
                                 .ToArrayAsync();
            }

            bool hasCourses = courses.Length > 0;

            return new AppResponse<Course[]>(
                hasCourses ? 200 : 404,
                hasCourses ? "Courses found." : "No courses found.",
                hasCourses,
                courses,
                null
            );
        }



        // Get enrolled courses based on the user's role
        public async Task<List<Enrollment.EnrolledCourses>> GetEnrolledCoursesAsync(int UID)
        {
            string role = await _userHelper.GetRoleByUserIdAsync(UID);

            var query = _context.Courses
                .Select(c => new Enrollment.EnrolledCourses
                {
                    ID = c.ID,
                    title = c.title,
                    description = c.description,
                    count = c.enrollments.Count
                });

            if (role != "admin")
            {
                query = query.OrderBy(c => c.title).Where(c => _context.Enrollments
                    .Any(e => e.courseID == c.ID && e.userID == UID));
            }

            return await query.OrderBy(c => c.title).ToListAsync();
        }

        // Enroll student in a course
        public async Task<AppResponse<int?>> EnrollAsync(int UID, int CID)
        {
            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.userID == UID && e.courseID == CID);
    
            if (alreadyEnrolled)
                return new AppResponse<int?>
                (
                    code:409,
                    message:"Course is already enrolled for the user.",
                    isSuccess:false,
                    errorData:null,
                    data:null
                );
    
            var enrollment = new Enrollment
            {
                userID = UID,
                courseID = CID,
                enrolledAt = DateTime.UtcNow
            };
    
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            return new AppResponse<int?>
            (
                code:200,
                message:"Course enrolled Successfully.",
                data:null,
                errorData:null,
                isSuccess:true
            );
        }

        public async Task<AppResponse<bool?>> AddCourse(Course course)
        {
            if (string.IsNullOrEmpty(course.title))
            {
                return new AppResponse<bool?>
                (
                    code : 400,
                    message:"Course title is required",
                    isSuccess:false,
                    data:null,
                    errorData:null
                );
            } else if (string.IsNullOrEmpty(course.description))
            {
                return new AppResponse<bool?>
                (
                    code:400,
                    message:"Course description is required",
                    isSuccess:false,
                    data:null,
                    errorData:null
                );
            }

            course.isActive = true;
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return new AppResponse<bool?>
            (
                code:200,
                message:"Course has been added successfully",
                isSuccess:true,
                data: null,
                errorData:null
            );
        }

        public async Task<AppResponse<bool?>> UpdateCourse(int id, Course course)
        {
            var existingCourse = await _context.Courses.FindAsync(id);

            if (existingCourse == null)
                return new AppResponse<bool?>
                (
                    code:404,
                    message:"Course does not exist",
                    isSuccess: true,
                    data:null,
                    errorData:null
                );

            _context.Entry(existingCourse).CurrentValues.SetValues(course);
            await _context.SaveChangesAsync();

            return new AppResponse<bool?>
            (
                code:200,
                message:"Course has been updated",
                isSuccess:true,
                data:null,
                errorData:null
            );
        }

        /*Not removing course, but disabling it. It is best to rather disable a course
         then to remove it when other users might still be enrolled*/
        public async Task<AppResponse<bool?>> RemoveCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
                return new AppResponse<bool?>
                (
                    code:404,
                    message:"Course does not exist",
                    isSuccess:false,
                    data:null,
                    errorData:null
                );

            // Set the course as inactive instead of deleting it
            course.isActive = false;

            _context.Courses.Update(course);
            await _context.SaveChangesAsync();

            return new AppResponse<bool?>
            (
                code:200,
                message:"Course has been disabled",
                data:null, 
                isSuccess:true, 
                errorData:null
            );
        }


        // Deregister student from a course
        public async Task<AppResponse<bool?>> DeregisterAsync(int UID, int CID)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.userID == UID && e.courseID == CID);

            if (enrollment == null)
                return new AppResponse<bool?>(
                    code:404,
                    message:"User not found.",
                    data:null,
                    isSuccess:false,
                    errorData:null
                );
    
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return new AppResponse<bool?>
            (
                code:200,
                message:"Course deregistered.",
                data:null,
                isSuccess:true,              
                errorData:null
            );
        }
        //Get enrollments plus details
        public async Task<AppResponse<List<Enrollment>>> GetEnrollmentsWithDetailsAsync(int UID)
        {
            string role = await _userHelper.GetRoleByUserIdAsync(UID);
            //Admin Enrollment Details
            List<Enrollment>? enrollments = new();
            if (role == "admin")
            {
                enrollments = await _context.Enrollments
                    .Include(e => e.course)
                    .ToListAsync();
                if (enrollments != null)
                    return new AppResponse<List<Enrollment>>
                    (
                        code:200,
                        message:"Enrollment details found.",
                        data:enrollments,
                        isSuccess:true,
                        errorData:null
                    );
                return new AppResponse<List<Enrollment>>
                (
                    code:404,
                    message:$"No enrollment details found for user id {UID}.",
                    data:enrollments,
                    isSuccess:false,
                    errorData:null
                );
            }
            //Else Role Student
            enrollments = await _context.Enrollments
                .Where(e => e.userID == UID)
                .Include(e => e.course)
                .ToListAsync();
            if (enrollments == null)
                return new AppResponse<List<Enrollment>>
                (
                    code:200,
                    message:"No enrollment details found",
                    data:enrollments,
                    isSuccess:false,
                    errorData:null
                );
            return new AppResponse<List<Enrollment>>
            (
                code:200,
                message:"Enrollment details found.",
                data:enrollments,
                isSuccess:true,
                errorData:null
            );
        }

        public async Task<AppResponse<bool?>> UpsertCourse(Course course)
        {
            Course? exists = await _context.Courses.FindAsync(course);
            if (exists == null)
            {
                //Check Formatting
                if (string.IsNullOrEmpty(course.title) || string.IsNullOrEmpty(course.description))
                {
                    return new AppResponse<bool?>
                    (
                        code:400,
                        message:"Course title and description are required",
                        isSuccess:false,
                        data:null,
                        errorData:null
                    );
                }
                //Insert
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                
                return new AppResponse<bool?>
                (
                    code:200,
                    message:"Course added successfully.",
                    isSuccess:true,
                    data:null,
                    errorData:null
                );

            }

            //Update
            _context.Entry(exists).CurrentValues.SetValues(course);
            await _context.SaveChangesAsync();

            return new AppResponse<bool?>
            (
                code:200,
                message:"Course has been updated",
                data:null, 
                isSuccess:true, 
                errorData:null
            );
        }

        public async Task<AppResponse> UpsertCourses(List<Course> courses)
        {
            var response = new AppResponse();

            for (var x = 0; x <= courses.Count; x++)
            {
                // Initialize lists
                if (!response.responses.ContainsKey(200)) 
                    response.responses[200] = new();

                if (!response.responses.ContainsKey(400)) 
                    response.responses[400] = new();

                // Validation
                if (string.IsNullOrWhiteSpace(courses[x].title) || string.IsNullOrWhiteSpace(courses[x].description))
                {
                    if (string.IsNullOrWhiteSpace(courses[x].title))
                    {
                        response.responses[400].Add(new CourseResponseItem
                        {
                            message = "Course title is required.",
                            course = courses[x]
                        });
                    }
                    else
                    {
                        response.responses[400].Add(new CourseResponseItem
                        {
                            message = "Course description is required.",
                            course = courses[x]
                        });
                    }
                        
                    continue;

                }

                // Check existing course
                Course? existingCourse = null;
                if (courses[x].ID > 0)
                {
                    existingCourse = await _context.Courses.FindAsync(courses[x].ID);
                }

                if (existingCourse == null)
                {
                    _context.Courses.Add(courses[x]);
                    response.responses[200].Add(new CourseResponseItem
                    {
                        message = "Course added successfully.",
                        course = courses[x]
                    });
                }
                else
                {
                    _context.Entry(existingCourse).CurrentValues.SetValues(courses[x]);
                    response.responses[200].Add(new CourseResponseItem
                    {
                        message = "Course updated successfully.",
                        course = courses[x]
                    });
                }
            }

            await _context.SaveChangesAsync();

            return response;
        }


    }
}

