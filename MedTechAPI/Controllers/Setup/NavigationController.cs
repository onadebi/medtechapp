using MedTechAPI.AppCore.Navigation.Interfaces;
using MedTechAPI.Domain.DTO.Navigation;
using MedTechAPI.Domain.Entities.ProfileManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnaxTools.Dto.Http;

namespace MedTechAPI.Controllers.Setup
{
    public class NavigationController : BaseSetupController
    {
        private readonly IMenuRepository _menuService;

        public NavigationController(IMenuRepository menuService)
        {
            _menuService = menuService;
        }

        [HttpGet(nameof(GetAllNavigationMenu))]
        [ProducesResponseType(typeof(GenResponse<List<MenuModules>>), 200)]
        public async Task<IActionResult> GetAllNavigationMenu()
        {
            var objResp = await _menuService.GetAllNavigationMenu();
            return StatusCode(objResp.StatCode, objResp);
        }

        [HttpGet(nameof(GetMenuNavigationByMenuId))]
        [ProducesResponseType(typeof(GenResponse<List<MenuModules>>), 200)]
        public async Task<IActionResult> GetMenuNavigationByMenuId(string MenuId)
        {
            var objResp = await _menuService.GetNavigationMenuByMenuId(MenuId);
            return StatusCode(objResp.StatCode, objResp);
        }

        [HttpPost(nameof(GetUserGroupRightsMenuNavigationByUserGroupIds))]
        [ProducesResponseType(typeof(GenResponse<List<MenuModules>>), 200)]
        public async Task<IActionResult> GetUserGroupRightsMenuNavigationByUserGroupIds(int[] UserGroupId)
        {
            var objResp = await _menuService.GetMenuAndSubNavigationsGroupRightsByUserGroupId(UserGroupId);
            return StatusCode(objResp.StatCode, objResp);
        }

        [HttpGet(nameof(GetUserMenuNavigation))]
        [ProducesResponseType(typeof(GenResponse<List<MenuModules>>), 200)]
        public async Task<IActionResult> GetUserMenuNavigation()
        {
            var objResp = await _menuService.GetUserMenuNavigation();
            return StatusCode(objResp.StatCode, objResp);
        }
    }
}
