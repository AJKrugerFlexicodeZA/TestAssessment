using MIDTIER.Models;

namespace StudentEnrollmentAPI.Interfaces
{
    public interface IEnrollmentService
    {
        AppResponse<List<Enrollment>> GetAllEnrollments(int updateUserId);
        AppResponse GetEnrollmentsByUserId(int user_id,int updateUserId);
        AppResponse<List<Enrollment.EnrolledCourses>> GetEnrolledCourses(int user_id, int updateUserId);
        AppResponse EnrollUser(int user_id, int course_id, int udpateUserId);
        AppResponse UnenrollUser(int user_id, int course_id, int updateUserId);
        AppResponse IsEnrolled(int user_id, int course_id, int updateUserId);
        AppResponse GetEnrollmentCountForCourse(int course_id, int updateUserId);
        AppResponse<List<EnrolledUsers>> GetUsersEnrolledByCourseId(int course_id, int updateUserId);
        AppResponse BulkUserCourseRemoval(List<int> userIds, int courseId, int updateUserId);

    }
}
