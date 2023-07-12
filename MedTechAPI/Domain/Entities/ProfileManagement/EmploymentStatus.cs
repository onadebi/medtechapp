using MedTechAPI.Domain.Entities.Extensions;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.Entities.ProfileManagement
{
    [Table(nameof(EmploymentStatus))]
    public class EmploymentStatus : CommonProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Status { get; set; }
    }
}
