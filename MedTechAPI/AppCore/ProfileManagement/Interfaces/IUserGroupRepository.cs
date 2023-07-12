using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.Entities.ProfileManagement;
using OnaxTools.Dto.Http;

namespace MedTechAPI.AppCore.Interfaces
{
    public interface IUserGroupRepository
    {
        Task<GenResponse<IEnumerable<UserGroup>>> GetAllUserGroups();
        Task<GenResponse<UserGroup>> InsertRole(UserRoleCreationDTO role);
        Task<GenResponse<List<UserGroup>>> GetUserGroupsByIds(int[] roleId);
        Task<GenResponse<int>> AddUserGroupRolesToUser(RolesToUserCreationDTO model, CancellationToken ct = default);
    }
}
