using MedTechAPI.Domain.DTO;
using OnaxTools.Dto.Identity;

namespace MedTechAPI.AppCore.Interfaces
{
    public interface IAppSessionContextRepository
    {
        AppSessionData<AppUser> GetUserDataFromSession();
        void ClearCurrentUserDataFromSession();
    }
}
