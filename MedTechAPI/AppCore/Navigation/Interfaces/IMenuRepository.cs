using MedTechAPI.Domain.DTO.Navigation;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using OnaxTools.Dto.Http;

namespace MedTechAPI.AppCore.Navigation.Interfaces
{
    public interface IMenuRepository
    {
        Task<GenResponse<List<MenuModules>>> GetAllNavigationMenu();
        Task<GenResponse<IEnumerable<AllNavigationMenuFlat>>> GetAllNavigationMenuFlat();
        Task<GenResponse<List<MenuModules>>> GetNavigationMenuByMenuId(string MenuId);
        Task<GenResponse<List<MainMenu>>> GetAllMenuModulesNavigation();
        Task<GenResponse<List<MainMenu>>> GetUserGroupsWithModuleAndUtilitiesAccess(CancellationToken ct = default);
        //Task<GenResponse<HashSet<MenuModules>>> GetUserMenuNavigation();
        Task<GenResponse<HashSet<MenuModules>>> GetMenuAndSubNavigationsGroupRightsByUserGroupId(int[] UserGroupId);
    }
}
