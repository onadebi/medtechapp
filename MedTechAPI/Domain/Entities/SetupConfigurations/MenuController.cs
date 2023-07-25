using MedTechAPI.Domain.Entities.Extensions;
using MedTechAPI.Domain.Entities.ProfileManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTechAPI.Domain.Entities.SetupConfigurations
{
    [Table(nameof(MenuController))]
    public class MenuController: CommonProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public int OrderPriority { get; set; }

        [Required]
        [StringLength(250)]
        public string ControllerName { get; set; }
        [Required]
        public int CompanyId { get; set; }
        [Required]
        [StringLength(250)]
        public string ControllerCode { get; set; }

        [Required]
        [StringLength(250)]
        public string UrlPath { get; set; }

        [Required]
        [StringLength(250)]
        public string DisplayName { get; set; }

        public string IconClass { get; set; }= "<span class='glyphicon glyphicon-barcode'></span>";

        public string ControllerDescription { get; set; }
        [Required]
        public bool IsSubscribed { get; set; } = true;

        [Required]
        public DateTime DateLastSubscribed { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsMenuDisplayed { get; set; } = false;

        public DateTime? DateLastUnSubscribed { get; set; } = null;

        #region Navigation properties
        public MedicCompanyDetail MedicCompanyDetail { get; set; }
        public ICollection<MenuControllerActions> MenuControllerActions { get; set; }
        //public ICollection<UserGroupMenuControllerActionPermissions> UserMenuControllerActionPermissions { get; set; }
        #endregion
    }
}
