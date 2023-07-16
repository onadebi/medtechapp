using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MedTechAPI.Domain.Entities.Extensions;

namespace MedTechAPI.Domain.Entities.SetupConfigurations
{
    [Table(nameof(MainMenu))]
    public  class MainMenu : CommonProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid MenuID { get; set; }

        [Required]
        [StringLength(250)]
        public string TitleDisplay { get; set; }

        public int OrderPriority { get; set; } = 0;

        //[Required]
        //[StringLength(250)]
        //public string ControllerName { get; set; } = String.Empty;
        [StringLength(250)]
        public string IconClass { get; set; }

        public bool IsSubscribed { get; set; } = true;

        [Required]
        public int CompanyId { get; set; }

        #region Navigation properties
        public virtual ICollection<SubMenu> SubMenus { get; set; }
        #endregion

        public MainMenu()
        {
            IconClass = "<span class='glyphicon glyphicon-barcode'></span>";
        }
    }
}
