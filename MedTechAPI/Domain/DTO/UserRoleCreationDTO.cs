using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.DTO
{
    public class UserRoleCreationDTO
    {
        [Required]
        [StringLength(100)]
        [BsonElement(nameof(RoleName))]
        public string RoleName { get; set; }


        [Required]
        [StringLength(250)]
        [BsonElement(nameof(RoleDescription))]
        public string RoleDescription { get; set; }
    }
}
