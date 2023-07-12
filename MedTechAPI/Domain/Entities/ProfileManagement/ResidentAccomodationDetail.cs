using MedTechAPI.Domain.Entities.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTechAPI.Domain.Entities.ProfileManagement
{
    [Table(nameof(ResidentAccomodationDetail))]
    public class ResidentAccomodationDetail : CommonProperties
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string ResidentName { get; set; }
    }
}
