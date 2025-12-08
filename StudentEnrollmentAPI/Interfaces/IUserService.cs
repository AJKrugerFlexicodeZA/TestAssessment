using MIDTIER.Models;

namespace StudentEnrollmentAPI.Interfaces
{
    public interface IUserService
    {
        AppResponse<List<UserList>> GetAll(int id);
        AppResponse GetById(int id,int updateUserId);
        AppResponse Add(User user,int updateUserId);
        AppResponse Update(User user, int updateUserId);
        AppResponse Delete(int id, int updateUserId);
        AppResponse ToggleActive(int id, int updateUserId);
    }
}
