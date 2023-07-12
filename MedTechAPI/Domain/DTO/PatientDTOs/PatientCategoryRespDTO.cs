using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MedTechAPI.Domain.DTO.PatientDTOs
{
    public class PatientCategoryRespDTO
    {
        public string CategoryName { get; set; }

        public int Id { get; set; }
    }

    public class PatientCategoryCreationDTO
    {
        public string CategoryName { get; set; }
    }

    public struct PatientCategoryUpdateDTO
    {
        [Required]
        public int Id { get; set; } = default;
        [Required]
        public string CategoryName { get; set; } = default;
        [Required]
        public bool IsActive { get; set; } 
        public PatientCategoryUpdateDTO()
        {
            IsActive = true;
        }
    }
}
