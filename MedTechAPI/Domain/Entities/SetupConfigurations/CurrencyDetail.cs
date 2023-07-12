using MedTechAPI.Domain.Entities.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTechAPI.Domain.Entities.SetupConfigurations
{
    [Table(nameof(CurrencyDetail))]
    public class CurrencyDetail : CommonProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string CurrencyCode { get; set; }
        [Required]
        [StringLength(100)]
        public string CurrencyName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitRate { get; set; }
        public int DecimalPlace { get; set; }
    }
}
