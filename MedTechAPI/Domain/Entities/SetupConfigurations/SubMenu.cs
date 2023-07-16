using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MedTechAPI.Domain.Entities.Extensions;

namespace MedTechAPI.Domain.Entities.SetupConfigurations
{
    [Table(nameof(SubMenu))]
    public class SubMenu : CommonProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid SubMenuId { get; set; }

        [Required]
        public Guid MenuID { get; set; }
        [MaxLength(250)]
        public string TitleDisplay { get; set; }
        public int OrderPriority { get; set; } = 0;
        public string UrlPath { get; set; } = "#";

        [StringLength(250)]
        public string IconClass { get; set; }

        [Required]
        public int CompanyId { get; set; }
        [Required]
        public bool IsSubscribed { get; set; } = true;
        #region Navigation properties
        public virtual MainMenu MainMenuId { get; set; }
        #endregion

        public SubMenu()
        {
            IconClass = "<span class='glyphicon glyphicon-barcode'></span>";
        }
    }
}
