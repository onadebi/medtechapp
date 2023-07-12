using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.DTO.LocationDTOs
{
    public class StateDetailsRespDTO
    {
        public int Id { get; set; }

        public string StateCode { get; set; }

        public string StateName { get; set; }
    }
    public class StateCreationRequestDTO
    {
        [Required]
        [StringLength(100)]
        public string StateCode { get; set; }

        [Required]
        [StringLength(100)]
        public string StateName { get; set; }
        [Required]
        public int CountryDetailId { get; set; }
    }

    public class UpdateStateDetailDTO: StateCreationRequestDTO
    {
        [Required]
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
