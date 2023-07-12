using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.DTO
{
    public class RolesToUserCreationDTO
    {
        [Required]
        public int UserProfileId { get; set; }
        [Required]
        public List<int> UserRoleIds { get; set; }
        [Required]
        public int CompanyId { get; set; }
    }
}
