using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.Entities.ProfileManagement
{
    [Table(nameof(UserGroup))]
    public class UserGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string GroupName { get; set; }

        [Required]
        public string GroupRight { get; set; } //CSV of Controller actions that the User group has rights to.

        public bool? AllowView { get; set; } = true;
        public bool? AllowNew { get; set; } = false;
        public bool? AllowEdit { get; set; } = false;
        public bool? AllowdDelete { get; set; } = false;
        public bool? AllowApprove { get; set; } = false;
        [Required]
        public int CompanyId { get; set; }

        [StringLength(250)]
        public string GroupDescription { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool? IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        #region Navigational Properties
        public ICollection<UserProfileGroup> UserProfileGroups { get; set; }
        //public HashSet<UserGroupMenuControllerActionPermissions> UserGroupMenuControllerActionPermissions { get; set; }

        #endregion

    }
}
