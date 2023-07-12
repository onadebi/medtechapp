using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.DTO
{
    public class EmailValidationDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        public string UserGuid { get; set; }
    }
}
