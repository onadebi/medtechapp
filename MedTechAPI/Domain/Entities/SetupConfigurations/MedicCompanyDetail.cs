using MedTechAPI.Domain.Entities.Extensions;
using MedTechAPI.Domain.Entities.ProfileManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTechAPI.Domain.Entities.SetupConfigurations
{
    [Table(nameof(MedicCompanyDetail))]
    public class MedicCompanyDetail : CommonProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(500)]
        public string CompanyName { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }

        [StringLength(500)]
        public string CompanyAddress { get; set; }


        [StringLength(500)]
        public string LogoPath { get; set; }

        #region Navigational properties
        public ICollection<BranchDetail> BranchDetail { get; set; }
        public ICollection<MenuController> MenuController { get; set; }
        public ICollection<MenuControllerActions> MenuControllerActions { get; set; }
        public ICollection<UserProfile> UserProfile { get; set; }
        public CountryDetail CountryDetail { get; set; }
        public StateDetail StateDetail { get; set; }
        #endregion

    }
}
