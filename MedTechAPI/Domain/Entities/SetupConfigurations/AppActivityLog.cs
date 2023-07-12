using MedTechAPI.Domain.Entities.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTechAPI.Domain.Entities.SetupConfigurations
{
    [Table(nameof(AppActivityLog))]
    public class AppActivityLog: CommonProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(250)]
        public string MethodOperation{ get; set; }

        //[Required]
        public string MessageData { get; set; }

        [Required]
        [StringLength(1000)]
        public string Identifier { get; set; }

        [StringLength(500)]
        public string UserGuid { get; set; }

        /// <summary>
        /// This should get value from the enum operations: MedTechAPI.Domain.Enums.AppActivityOperation
        /// </summary>
        [Required]
        [StringLength(250)]
        public string Operation { get; set; }

        [StringLength(1000)]
        public string Data { get; set; }
        public bool? IsSuccessfulOperation { get; set; } = true;

        #region Navigation properties

        #endregion


    }
}
