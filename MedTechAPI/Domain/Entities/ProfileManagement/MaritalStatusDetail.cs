using MedTechAPI.Domain.Entities.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTechAPI.Domain.Entities.ProfileManagement
{
    [Table(nameof(MaritalStatusDetail))]
    public class MaritalStatusDetail : CommonProperties
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string MaritalTitle { get; set; }
    }
}
