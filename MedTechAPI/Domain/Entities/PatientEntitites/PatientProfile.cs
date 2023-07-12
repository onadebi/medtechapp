using MedTechAPI.Domain.Entities.Extensions;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTechAPI.Domain.Entities.PatientEntitites
{
    [Table(nameof(PatientProfile))]
    public class PatientProfile: CommonProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public int SalutationId { get; set; }

        [Required]
        [StringLength(250)]
        public string FirstName { get; set; }

        [StringLength(250)]
        public string MiddleName { get; set; }

        [Required]
        [StringLength(250)]
        public string LastName { get; set; }

        [StringLength(maximumLength: 500)]
        public string PhotoUrlPath { get; set; }

        public DateTime Dob { get; set; }

        [StringLength(100)]
        public string PatientCode { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set;}

        [Required]
        public int CountryId { get; set; }

        [Required]
        public int MedicBranchId { get; set; }

        [Required]
        public int StateId { get; set; }

        [StringLength(1500)]
        public string Address { get; set; }


        [Required]
        [StringLength(250)]
        public Guid PatientUserGuid { get; set; } = Guid.NewGuid();

        #region Navigation properties
        public ICollection<PatientNextOfKin> PatientNextOfKins { get; set; }
        public BranchDetail BranchDetail { get; set; }
        #endregion
    }
}
