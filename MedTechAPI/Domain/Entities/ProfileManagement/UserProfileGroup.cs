using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.Entities.ProfileManagement
{
    [Table(nameof(UserProfileGroup))]
    public class UserProfileGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int UserGroupId { get; set; }

        [Required]
        public int UserProfileId { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;


        #region Navigational Properties
        public UserProfile UserProfile { get; set; }

        public UserGroup UserGroups { get; set; }
        #endregion
    }
}
