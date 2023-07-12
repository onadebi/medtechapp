using AutoMapper;
using Common.DbAccess;
using Common.Interface;
using Dapper;
using Hangfire;
using MedTechAPI.AppCore.AppGlobal.Repository;
using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.AppCore.Navigation.Interfaces;
using MedTechAPI.AppCore.Repository;
using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.DTO.Navigation;
using MedTechAPI.Domain.Entities.ProfileManagement;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using MedTechAPI.Domain.Enums;
using MedTechAPI.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnaxTools.Dto.Http;
using OnaxTools.Dto.Identity;
using OnaxTools.Enums.Http;
using System.Runtime.CompilerServices;

namespace MedTechAPI.AppCore.Navigation.Repository
{
    public class MenuRepository : IMenuRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserServiceRepository> _logger;
        private readonly IAppActivityLogRepository _activityLogRepo;
        private readonly IAppSessionContextRepository _appSession;
        private readonly ISqlDataAccess _sqlData;
        private readonly IOptions<AppSettings> _appsettings;
        private readonly ICacheService _cacheService;
        private readonly IUserGroupRepository _userGroupRepo;
        private readonly IMapper _mapper;

        private string _cacheMainMenuList(string MenuSvc = "MenuCache") => $"{MenuSvc}{nameof(_cacheMainMenuList)}";
        private string _cacheSubMenuList(string MenuSvc = "MenuCache") => $"{MenuSvc}{nameof(_cacheSubMenuList)}";
        private string _cacheSubMenuListFlat(string MenuSvc = "MenuCache") => $"{MenuSvc}{nameof(_cacheSubMenuListFlat)}";
        public MenuRepository(AppDbContext context
            , ILogger<UserServiceRepository> logger
            , IAppActivityLogRepository activityLogRepo
            , IAppSessionContextRepository appSession
            , ISqlDataAccess sqlData
            //, IMessageRepository msgRepo
            , IOptions<AppSettings> appsettings
            , ICacheService cacheService
            , IUserGroupRepository userGroupRepo
            , IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _activityLogRepo = activityLogRepo;
            _appSession = appSession;
            _sqlData = sqlData;
            _appsettings = appsettings;
            _cacheService = cacheService;
            _userGroupRepo = userGroupRepo;
            _mapper = mapper;
        }
                
        public async Task<GenResponse<List<MenuModules>>> GetAllNavigationMenu()
        {
            GenResponse<List<MenuModules>> objResp = new GenResponse<List<MenuModules>>();
            objResp.Result = await _cacheService.GetData<List<MenuModules>>(_cacheMainMenuList());
            if (objResp == null || objResp.Result == null || !objResp.Result.Any())
            {
                try
                {
                    objResp.Result = await _context.MenuController.Include(m => m.MenuControllerActions).AsNoTracking().Select(m => new MenuModules
                    {
                        Id = m.Id,
                        OrderPriority = m.OrderPriority,
                        ControllerName = m.ControllerName,
                        ControllerCode = m.ControllerCode,
                        UrlPath = m.UrlPath,
                        DisplayName = m.DisplayName,
                        IconClass = m.IconClass,
                        ControllerDescription = m.ControllerDescription,
                        //MenuActions = _mapper.Map<HashSet<MenuActions>>(m.MenuControllerActions)
                        MenuActions = m.MenuControllerActions.Select(m => new MenuActions
                        {
                            Id = m.Id,
                            OrderPriority = m.OrderPriority,
                            ActionDescription = m.ActionDescription,
                            ActionName = m.ActionName,
                            DisplayName = m.DisplayName,
                            IconClass = m.IconClass,
                            MenuControllerId = m.MenuControllerId,
                            UrlPath = m.UrlPath,
                            AllowAnonymous = m.AllowAnonymous,
                            CompanyId = m.CompanyId,
                            MenuModules = null,
                        }).AsList()
                    }).OrderBy(m => m.ControllerName).ToListAsync();

                    if (objResp != null)
                    {
                        objResp.IsSuccess = true;
                        _ = _cacheService.SetData<List<MenuModules>>(_cacheMainMenuList(), objResp.Result, 60 * 60);
                    }
                }
                catch (Exception ex)
                {
                    LogRepositoryError<List<MenuModules>>(objResp, nameof(GetAllNavigationMenu), ex, AppActivityOperationEnum.UserActivity);
                }
            }
            else
            {
                objResp.IsSuccess = true;
            }
            return objResp;
        }
        public async Task<GenResponse<IEnumerable<AllNavigationMenuFlat>>> GetAllNavigationMenuFlat()
        {
            GenResponse<IEnumerable<AllNavigationMenuFlat>> objResp = new ();
            objResp.Result = await _cacheService.GetData<List<AllNavigationMenuFlat>>(_cacheSubMenuListFlat());
            if (objResp == null || objResp.Result == null || !objResp.Result.Any())
            {
                try
                {
                    string query = "select mc.\"ControllerName\",mc.\"IsSubscribed\", mca.* from public.\"MenuControllerActions\" mca inner join public.\"MenuController\" mc on mca.\"MenuControllerId\" = mc.\"Id\" order by mc.\"Id\"";
                    objResp.Result= await _sqlData.GetData<AllNavigationMenuFlat, dynamic>(query, new { });
                    if(objResp.Result != null && objResp.Result.Any())
                    {
                        objResp.IsSuccess = true;
                        await _cacheService.SetData<List<AllNavigationMenuFlat>>(_cacheSubMenuListFlat(),objResp.Result.ToList(),60*60);
                    }
                }
                catch (Exception ex)
                {
                    LogRepositoryError<IEnumerable<AllNavigationMenuFlat>>(objResp, nameof(GetAllNavigationMenu), ex, AppActivityOperationEnum.UserActivity);
                }
            }
            else
            {
                objResp.IsSuccess = true;
            }
            return objResp;
        }
        public async Task<GenResponse<List<MenuModules>>> GetAllTopLevelOnlyNavigationMenu()
        {
            GenResponse<List<MenuModules>> objResp = new GenResponse<List<MenuModules>>();
            objResp.Result = await _cacheService.GetData<List<MenuModules>>(_cacheMainMenuList("TopLevelOnly"));
            if (objResp == null || objResp.Result == null || !objResp.Result.Any())
            {
                try
                {
                    objResp.Result = await _context.MenuController.AsNoTracking().Select(m => new MenuModules
                    {
                        Id = m.Id,
                        OrderPriority = m.OrderPriority,
                        ControllerName = m.ControllerName,
                        ControllerCode = m.ControllerCode,
                        UrlPath = m.UrlPath,
                        DisplayName = m.DisplayName,
                        IconClass = m.IconClass,
                        ControllerDescription = m.ControllerDescription,
                        MenuActions = null
                    }).OrderBy(m => m.OrderPriority).ToListAsync();

                    if (objResp != null)
                    {
                        objResp.IsSuccess = true;
                        _ = _cacheService.SetData<List<MenuModules>>(_cacheMainMenuList("TopLevelOnly"), objResp.Result, 60 * 60);
                    }
                }
                catch (Exception ex)
                {
                    LogRepositoryError<List<MenuModules>>(objResp, nameof(GetAllTopLevelOnlyNavigationMenu), ex, AppActivityOperationEnum.UserActivity);
                }
            }
            else
            {
                objResp.IsSuccess = true;
            }
            return objResp;
        }
        public async Task<GenResponse<List<MenuActions>>> GetAllNavigationSubMenu()
        {
            GenResponse<List<MenuActions>> objResp = new GenResponse<List<MenuActions>>();
            objResp.Result = await _cacheService.GetData<List<MenuActions>>(_cacheSubMenuList());
            if (objResp == null || objResp.Result == null || !objResp.Result.Any())
            {
                try
                {
                    objResp.Result = await _context.MenuControllerActions.Include(m => m.MenuController).AsNoTracking().Select(m => new MenuActions
                    {
                        Id = m.Id,
                        OrderPriority = m.OrderPriority,
                        ActionDescription = m.ActionDescription,
                        ActionName = m.ActionName,
                        DisplayName = m.DisplayName,
                        IconClass = m.IconClass,
                        MenuControllerId = m.MenuControllerId,
                        UrlPath = m.UrlPath,
                        AllowAnonymous = m.AllowAnonymous,
                        MenuModules = null,
                    }).ToListAsync();
                    if (objResp != null)
                    {
                        objResp.IsSuccess = true;
                        _ = _cacheService.SetData<List<MenuActions>>(_cacheSubMenuList(), objResp.Result, 60 * 60);
                    }
                }
                catch (Exception ex)
                {
                    LogRepositoryError<List<MenuActions>>(objResp, nameof(GetAllNavigationSubMenu), ex, AppActivityOperationEnum.UserActivity);
                }
            }
            else
            {
                objResp.IsSuccess = true;
            }
            return objResp;
        }
        public async Task<GenResponse<List<MenuModules>>> GetNavigationMenuByMenuId(string MenuId)
        {
            GenResponse<List<MenuModules>> objResp = new GenResponse<List<MenuModules>>();
            Guid MenuGuid;
            if (!Guid.TryParse(MenuId, out MenuGuid))
            {
                return GenResponse<List<MenuModules>>.Failed("Invalid menu Id format supplied.");
            }

            try
            {
                objResp = await GetAllNavigationMenu();
                if (objResp != null && objResp.IsSuccess && objResp.Result.Any())
                {
                    objResp.Result = objResp.Result.Where(m => m.Id == MenuGuid).ToList();
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<List<MenuModules>>(objResp, nameof(GetNavigationMenuByMenuId), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<HashSet<MenuModules>>> GetUserMenuNavigation()
        {
            AppSessionData<AppUser> user = _appSession.GetUserDataFromSession();
            GenResponse<HashSet<MenuModules>> objResp = new GenResponse<HashSet<MenuModules>>() { Result = new() };
            try
            {
                
            }
            catch (Exception ex)
            {
                LogRepositoryError<HashSet<MenuModules>>(objResp, nameof(GetMenuAndSubNavigationsGroupRightsByUserGroupId), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<HashSet<MenuModules>>> GetMenuAndSubNavigationsGroupRightsByUserGroupId(int[] UserGroupId)
        {
            GenResponse<HashSet<MenuModules>> objResp = new GenResponse<HashSet<MenuModules>>() { Result = new() };
            try
            {
                GenResponse<List<UserGroup>> userGroup = await _userGroupRepo.GetUserGroupsByIds(UserGroupId);
                if (userGroup == null || userGroup.Result == null)
                {
                    return GenResponse<HashSet<MenuModules>>.Failed("No usergroup found.", StatusCodeEnum.NotFound);
                }
                List<string> controllerActions = new();
                userGroup.Result.ForEach(m =>
                {
                    if (!string.IsNullOrWhiteSpace(m.GroupRight))
                    {
                        controllerActions.AddRange(m.GroupRight.Split(','));
                    }
                });
                //string[] controllerActions = userGroup.Result..GroupRight.Split(',');
                if (controllerActions.Count <= 0 || string.IsNullOrWhiteSpace(controllerActions[0]))
                {
                    return GenResponse<HashSet<MenuModules>>.Failed($"No associated group rights found.", StatusCodeEnum.OK);
                }
                var allSubMenuActions = await GetAllNavigationSubMenu();
                var topLevelMenu = await GetAllTopLevelOnlyNavigationMenu();

                List<MenuActions> userSubMenu = new();
                if (allSubMenuActions != null && allSubMenuActions.IsSuccess & allSubMenuActions.Result.Any())
                {
                    userSubMenu = allSubMenuActions.Result.Where(m => controllerActions.Contains(m.ActionName)).ToList();
                }
                topLevelMenu.Result.ForEach(k =>
                {
                    var subMenuActions = userSubMenu.Where(m => m.MenuControllerId == k.Id).ToList();
                    if (subMenuActions != null && subMenuActions.Any())
                    {
                        MenuModules menuThatHasSubMenu = new();
                        menuThatHasSubMenu = k;
                        menuThatHasSubMenu.MenuActions = subMenuActions;
                        objResp.Result.Add(menuThatHasSubMenu);
                    }
                });
                if(objResp != null && objResp.Result.Count > 0)
                {
                    objResp.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<HashSet<MenuModules>>(objResp, nameof(GetMenuAndSubNavigationsGroupRightsByUserGroupId), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }
      
        #region HELPERS
        private void LogRepositoryError<T>(GenResponse<T> objResp, string Identifier, Exception ex, AppActivityOperationEnum operation = AppActivityOperationEnum.SystemOperation, AppActivityLogTypeEnum logType = AppActivityLogTypeEnum.ErrorLog, [CallerMemberName] string caller = "")
        {
            _logger.LogError(ex, ex != null ? ex.Message : null);
            objResp.Error = ex.Message;
            objResp.IsSuccess = false;
            objResp.StatCode = (int)StatusCodeEnum.ServerError;
            AppActivityLog log = new()
            {
                Data = ex.Message,
                Identifier = Identifier,
                MessageData = logType.ToString(),
                Operation = operation.ToString(),
                IsSuccessfulOperation = false,
            };
            AppSessionData<AppUser> user = _appSession.GetUserDataFromSession();
            BackgroundJob.Enqueue(() => _activityLogRepo.LogToDatabase(log, user, caller));
        }

        private async Task ClearAllMenuCache()
        {
            await _cacheService.RemoveData(_cacheMainMenuList());
            await _cacheService.RemoveData(_cacheSubMenuList());
            await _cacheService.RemoveData(_cacheMainMenuList("TopLevelOnly"));
        }

        #endregion
    }
}
