using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTechAPI.Domain.Entities.SetupConfigurations
{
    [Table(nameof(MessageBox))]
    public class MessageBox
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [StringLength(maximumLength: 100)]
        public string AppName { get; set; }

        [Required]
        [StringLength(maximumLength: 100)]
        public string Operation { get; set; }

        /// <summary>
        /// Email || SMS || WhatsApp || ... This defaults to Email
        /// </summary>
        [StringLength(maximumLength: 100)]
        public string Channel { get; set; } = "Email";

        [StringLength(maximumLength: 250)]
        public string Description { get; set; }

        [StringLength(maximumLength: 500)]
        public string MessageData { get; set; }

        #region
        //TODO: Move below properties to seperate table TokenBox
        //[BsonElement(nameof(ExpireAt))]
        //public DateTime ExpireAt { get; set; } = DateTime.UtcNow.AddDays(1);
        #endregion

        [Required]
        [StringLength(maximumLength: 100)]
        public string EmailReceiver { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiredAt { get; set; } = DateTime.UtcNow.AddDays(1);

        [StringLength(maximumLength: 100)]
        public string UserId { get; set; }

        [Required]
        public bool ForQueue { get; set; } = false;

        [Required]
        public bool IsProcessed { get; set; } = false;

        [Required]
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// Default status of 0 indicates not yet used. -1: Pending/Expired. 1: Used/Processed
        /// </summary>
        [Required]
        public short CompletedStatus { get; set; } = 0;
    }
}
