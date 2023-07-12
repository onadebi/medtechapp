using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTechAPI.Domain.Entities.SetupConfigurations
{
    [Table(nameof(EmailConfig))]
    public class EmailConfig
    {
        [Required]
        [StringLength(250)]
        public string SenderName { get; set; }
        [Required]
        [Key]
        [StringLength(250)]
        public string SenderEmail { get; set; }
        [Required]
        [StringLength(250)]
        public string SmtpService { get; set; }
        [Required]
        [StringLength(250)]
        public string SmtpPassword { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpHost { get; set; }
        public bool IsDevelopment { get; set; }
    }
}
