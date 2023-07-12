using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.DTO
{
    public class EmailModelDTO
    {
        [Required]
        public string ReceiverEmail { get; set; }
        [Required]
        public string EmailSubject { get; set; }
        [Required]
        public string EmailBody { get; set; }
    }

    public class EmailModelWithDataDTO : EmailModelDTO
    {
        [Required]
        public Dictionary<string, string> EmailBodyData { get; set; }
    }
}
