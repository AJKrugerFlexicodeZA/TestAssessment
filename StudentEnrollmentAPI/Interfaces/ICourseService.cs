using MIDTIER.Models;

namespace StudentEnrollmentAPI.Interfaces
{
    public interface ICourseService
    {
        AppResponse GetAll(int updateUserId);
        AppResponse? GetById(int id, int updateUserId);
        AppResponse Add(Course course, int updateUserId);
        AppResponse Update(Course course, int updateUserId);
        AppResponse Delete(int id, int updateUserId);
    }
}
