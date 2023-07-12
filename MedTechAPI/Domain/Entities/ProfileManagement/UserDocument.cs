using MedTechAPI.Domain.Entities.Extensions;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MedTechAPI.Domain.Entities.SetupConfigurations;

namespace MedTechAPI.Domain.Entities.ProfileManagement
{
    [Table(nameof(UserDocument))]
    public class UserDocument : CommonProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong DocId { get; set; }

        [Required]
        public int UserProfileId { get; set; }

        [Required]
        public int AppDocumentId { get; set; }

        [Required]
        [StringLength(500)]
        public string DocumentName { get; set; }

        [StringLength(500)]
        public string FullDocumentNamePath { get; set; }

        /// <summary>
        /// This should be in list of acceptable types from enum [AppDocumentExtensionTypes]
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ExtensionType { get; set; }

        #region Navaigational properties
        public AppDocuments AppUserDocuments { get; set; }
        #endregion
    }
}
