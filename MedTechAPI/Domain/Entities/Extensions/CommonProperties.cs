using MedTechAPI.Domain.Config;
using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.Entities.Extensions
{
    public class CommonProperties
    {
        public virtual bool ActiveStatus { get; set; } = true;
        public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(250)]
        public virtual string CreatedBy { get; set; } = AppConstants.AppSystem;
        [StringLength(250)]
        public virtual string ModifiedBy { get; set; } = AppConstants.AppSystem;
        [StringLength(250)]
        public virtual string ApprovedBy { get; set; } = AppConstants.AppSystem;
        [StringLength(250)]
        public virtual string Ip { get; set; } = AppConstants.AppSystem;
    }
}
