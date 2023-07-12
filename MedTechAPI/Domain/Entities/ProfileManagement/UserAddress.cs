using MedTechAPI.Domain.Entities.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTechAPI.Domain.Entities.ProfileManagement
{
    [Table(nameof(UserAddress))]
    public class UserAddress: CommonProperties
    {
    }
}
