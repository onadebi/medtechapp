using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MedTechAPI.Domain.Entities.Extensions;
using MedTechAPI.Domain.Entities.ProfileManagement;

namespace MedTechAPI.Domain.Entities.SetupConfigurations
{
    [Table(nameof(AppDocuments))]
    public class AppDocuments : CommonProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DocId { get; set; }

        [Required]
        [StringLength(250)]
        public string DocumentName { get; set; }

        [StringLength(250)]
        public string DocumentDescription { get; set; }

        [Required]
        [StringLength(250)]
        public string[] DocumentAllowedFormats { get; set; } = Array.Empty<string>();

        [Required]
        public decimal MaxMbFileSize { get; set; }

        [Required]
        public bool IsForPatient { get; set; } = true;

        [Required]
        public bool IsForStaff { get; set; } = false;

        public int Category { get; set; } = 0;

        #region Navaigational properties
        public ICollection<UserDocument> UserAppDocument { get; set; }
        #endregion

    }

}
