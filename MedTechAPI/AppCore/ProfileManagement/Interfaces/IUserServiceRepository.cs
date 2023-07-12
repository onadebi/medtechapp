using MedTechAPI.Domain.DTO;
using OnaxTools.Dto.Http;

namespace MedTechAPI.AppCore.Interfaces
{
    public interface IUserServiceRepository
    {
        Task<GenResponse<bool>> ResetForgottenPasswordRequest(string Email, CancellationToken ct = default);
        Task<GenResponse<bool>> IsAnyUserProfileExist();
        Task<GenResponse<bool>> ResetForgottenPassword(UserAuthChangeForgottenPasswordDto user, CancellationToken ct = default);
        Task<GenResponse<List<AppUser>>> GetAllUsers();
        Task<GenResponse<AppUser>> GetUserWithRolesByUserId(string UserGuid);
        Task<GenResponse<AppUser>> GetUserWithRolesByEmail(string Email);
        Task<GenResponse<UserLoginResponse>> RegisterAdmin(UserModelCreateDto user);
        Task<GenResponse<UserLoginResponse>> RegisterUser(UserModelCreateDto user);
        Task<GenResponse<UserLoginResponse>> Login(UserLoginDto userLogin);
        Task<GenResponse<UserLoginResponse>> EmailRegistrationValidation(EmailValidationDto user);
    }
}
