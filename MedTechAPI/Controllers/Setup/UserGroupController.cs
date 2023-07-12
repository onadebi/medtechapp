using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.Domain.Entities.ProfileManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnaxTools.Dto.Http;

namespace MedTechAPI.Controllers.Setup;

public class UserGroupController : BaseSetupController
{
    private readonly IUserGroupRepository _userGroupRepo;

    public UserGroupController(IUserGroupRepository userGroupRepo)
    {
        this._userGroupRepo = userGroupRepo;
    }

    [HttpGet(nameof(FetchAllUserGroups))]
    [ProducesResponseType(typeof(GenResponse<IEnumerable<UserGroup>>), 200)]
    public async Task<IActionResult> FetchAllUserGroups()
    {
        return Ok(await _userGroupRepo.GetAllUserGroups());
    }
}
