using MedTechAPI.Domain.Entities.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTechAPI.Domain.Entities.ProfileManagement
{
    [Table(nameof(Salutation))]
    public class Salutation : CommonProperties
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string SalutationName { get; set; }

        #region Navigation properties
        public ICollection<UserProfile> UserProfile { get; set; }
        #endregion
    }
}
