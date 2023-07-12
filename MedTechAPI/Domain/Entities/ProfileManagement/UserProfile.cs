using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MedTechAPI.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using MedTechAPI.Domain.Entities.SetupConfigurations;

namespace MedTechAPI.Domain.Entities.ProfileManagement
{
    [Table(nameof(UserProfile))]
    public class UserProfile: IdentityUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SalutationId { get; set; }

        [Required]
        [StringLength(maximumLength: 250)]
        public string FirstName { get; set; }

        [StringLength(maximumLength: 250)]
        public string MiddleName { get; set; }
        
        [Required]
        [StringLength(maximumLength: 250)]
        public string LastName { get; set; }

        public DateTime Dob { get; set; }

        [Required]
        public int GenderCategoryId { get; set; }


        [StringLength(maximumLength: 500)]
        public string PhotoUrlPath { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(maximumLength: 250)]
        public override string Email { get; set; }
        [Required]
        [StringLength(maximumLength: 500)]
        public string Password { get; set; }

        [Required]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime DateLastUpdated { get; set; } = DateTime.UtcNow;
        [Required]
        public bool IsEmailConfirmed { get; set; } = false;
        public bool IsDeactivated { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        [StringLength(maximumLength: 250)]
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();

        [Required]
        public int MedicBranchId { get; set; }
        [Required]
        public int MedicCompanyId { get; set; }

        public UserProfile()
        {
            UserProfileGroups = new HashSet<UserProfileGroup>();
        }
        #region Navigational properties
        public HashSet<UserProfileGroup> UserProfileGroups { get; set; }
        public HashSet<UserNextOfKin> UserNextOfKin { get; set; }
        public BranchDetail BranchDetail { get; set; }
        public MedicCompanyDetail MedicCompanyDetail { get; set; }
        public GenderCategory GenderCategory { get; set; }
        public Salutation Salutation { get; set; }
        #endregion
    }
}
