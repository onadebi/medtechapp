using MedTechAPI.Domain.Entities.Extensions;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.Entities.SetupConfigurations
{
    [Table(nameof(CountryDetail))]
    public class CountryDetail: CommonProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CountryCode { get; set; }

        [Required]
        [StringLength(100)]
        public string CountryName { get; set; }

        #region Navigation properties
        public ICollection<StateDetail> StateDetails { get; set; }
        public ICollection<MedicCompanyDetail> MedicCompanyDetail { get; set; }
        #endregion
    }
}
