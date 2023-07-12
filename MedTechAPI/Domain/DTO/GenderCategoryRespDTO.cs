using MedTechAPI.Domain.DTO.LocationDTOs;
using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.DTO
{
    public class GenderCategoryCreationDTO
    {
        [Required]
        [StringLength(50)]
        public string GenderName { get; set; }
    }

    public class GenderCategoryRespDTO: GenderCategoryCreationDTO
    {
        public int GenderId { get; set; }

    }


    public class GenderCategoryUpdateRequestDTO : GenderCategoryRespDTO
    {
        public bool IsActive { get; set; } = true;
    }
}
