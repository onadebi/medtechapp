using OnaxTools.Dto.Identity;

namespace MedTechAPI.Domain.DTO
{
    public class AppUser: AppUserIdentity
    {
        public int CompanyId { get; set; }
    }
}
