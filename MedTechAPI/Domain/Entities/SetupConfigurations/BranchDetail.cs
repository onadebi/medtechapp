using MedTechAPI.Domain.Entities.Extensions;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MedTechAPI.Domain.Entities.PatientEntitites;
using MedTechAPI.Domain.Entities.ProfileManagement;

namespace MedTechAPI.Domain.Entities.SetupConfigurations
{
    [Table(nameof(BranchDetail))]
    public class BranchDetail : CommonProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(500)]
        public string BranchName { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        [StringLength(500)]
        public string BranchAddress { get; set; }
        public int CompanyId { get; set; }
        [StringLength(500)]
        public string LogoPath { get; set; }

        #region Navigational properties
        public MedicCompanyDetail CompanyDetail { get; set; }
        public ICollection<UserProfile> UserProfile { get; set; }
        public ICollection<PatientProfile> PatientProfile { get; set; }
        #endregion
    }
}
