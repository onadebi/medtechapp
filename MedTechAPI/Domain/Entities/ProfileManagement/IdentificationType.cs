using MedTechAPI.Domain.Entities.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTechAPI.Domain.Entities.ProfileManagement
{
    [Table(nameof(IdentificationType))]
    public class IdentificationType : CommonProperties
    {
        [Key]
        public int IdentityKey { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Identification")]
        public string IdentityName { get; set; }
    }
}
