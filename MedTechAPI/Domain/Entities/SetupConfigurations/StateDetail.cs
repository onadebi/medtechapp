using MedTechAPI.Domain.Entities.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTechAPI.Domain.Entities.SetupConfigurations
{
    [Table(nameof(StateDetail))]
    public class StateDetail : CommonProperties
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string StateCode { get; set; }

        [Required]
        [StringLength(100)]
        public string StateName { get; set; }
        [Required]
        public int CountryDetailId { get; set; }

        #region Navigational properties
        public CountryDetail CountryDetail { get; set; }
        public ICollection<MedicCompanyDetail> MedicCompanyDetail { get; set; }
        #endregion
    }
}
