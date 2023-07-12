using MedTechAPI.Domain.DTO.Navigation;
using OnaxTools.Dto.Http;

namespace MedTechAPI.AppCore.Navigation.Interfaces
{
    public interface IMenuRepository
    {
        Task<GenResponse<List<MenuModules>>> GetAllNavigationMenu();
        Task<GenResponse<IEnumerable<AllNavigationMenuFlat>>> GetAllNavigationMenuFlat();
        Task<GenResponse<List<MenuModules>>> GetNavigationMenuByMenuId(string MenuId);
        Task<GenResponse<HashSet<MenuModules>>> GetUserMenuNavigation();
        Task<GenResponse<HashSet<MenuModules>>> GetMenuAndSubNavigationsGroupRightsByUserGroupId(int[] UserGroupId);
    }
}
