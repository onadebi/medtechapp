using MedTechAPI.Domain.Entities.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTechAPI.Domain.Entities.ProfileManagement
{
    [Table(nameof(GenderCategory))]
    public class GenderCategory: CommonProperties
    {
        [Key]
        public int GenderId { get; set; }

        [Required]
        [StringLength(50)]
        public string GenderName { get; set; }

        #region Navigational properties
        public ICollection<UserProfile> UserProfile { get; set; }
        #endregion
    }
}
