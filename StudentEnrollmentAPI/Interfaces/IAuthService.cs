using MIDTIER.Models;

namespace StudentEnrollmentAPI.Interfaces
{
    public interface IAuthService
    {
        AuthResponse Login(Login request);
        AppResponse Register(Register request);
    }
}
