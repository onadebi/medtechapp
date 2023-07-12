using Common.Interface;
using Hangfire;
using MedTechAPI.AppCore.AppGlobal.Interface;
using MedTechAPI.AppCore.AppGlobal.Repository;
using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.DTO.LocationDTOs;
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

namespace MedTechAPI.AppCore.ProfileManagement.Repository
{
    public interface IGenderCategoryRespository : ICommonBaseRepo<GenderCategoryCreationDTO, GenderCategoryUpdateRequestDTO, GenderCategoryRespDTO>
    {
        Task<GenResponse<IEnumerable<GenderCategory>>> FetchAllGenderCategories();
    }
    public class GenderCategoryRespository : IGenderCategoryRespository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GenderCategoryRespository> _logger;
        private readonly IAppActivityLogRepository _activityLogRepo;
        private readonly IAppSessionContextRepository _appSession;
        private readonly ICacheService _cache;
        private readonly AppSettings _appsettings;
        private GenericRepository<GenderCategory> _genericRepo;
        public GenderCategoryRespository(AppDbContext context
            , ILogger<GenderCategoryRespository> logger
            , IAppActivityLogRepository activityLogRepo
            , IAppSessionContextRepository appSession
            , ICacheService cache
            , IOptions<AppSettings> appsettings)
        {
            _context = context;
            _logger = logger;
            _activityLogRepo = activityLogRepo;
            _appSession = appSession;
            _cache = cache;
            _appsettings = appsettings.Value;
            if (_genericRepo == null)
            {
                _genericRepo = new GenericRepository<GenderCategory>(_context);
            }
        }

        public Task<GenResponse<int>> Add(GenderCategoryCreationDTO model)
        {
            throw new NotImplementedException();
        }

        public async Task<GenResponse<List<GenderCategoryRespDTO>>> FetchAll(bool includeInActive = false)
        {
            GenResponse<List<GenderCategoryRespDTO>> objResp = new() { Result = new() };
            try
            {
                if (!includeInActive)
                {
                    objResp.Result = await _cache.GetData<List<GenderCategoryRespDTO>>($"{_appsettings.AppName}_AllGenderCategories");
                    objResp.IsSuccess = objResp.Result != null && objResp.Result.Any();
                }
                if (objResp.Result == null || !objResp.Result.Any())
                {
                    if (includeInActive == true)
                    {
                        objResp.Result = await _context.GenderCategories
                            .Select(m =>
                               new GenderCategoryRespDTO
                               {
                                   GenderId = m.GenderId,
                                   GenderName = m.GenderName,
                               }).AsNoTracking().ToListAsync();
                    }
                    else
                    {
                        objResp.Result = await _context.GenderCategories.Where(m => m.ActiveStatus == true).Select(m =>
                            new GenderCategoryRespDTO
                            {
                                GenderId = m.GenderId,
                                GenderName = m.GenderName,
                            }).AsNoTracking().ToListAsync();
                    }
                    if (objResp.Result != null && objResp.Result.Any())
                    {
                        objResp.IsSuccess = true;
                        await _cache.SetData<IEnumerable<GenderCategoryRespDTO>>($"{_appsettings.AppName}_AllGenderCategories", objResp.Result, 60 * 60 * 12);
                    }
                    else
                    {
                        objResp.IsSuccess = false;
                        objResp.Message = $"No gender list details found.";
                    }
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<List<GenderCategoryRespDTO>>(objResp, nameof(FetchAll), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<IEnumerable<GenderCategory>>> FetchAllGenderCategories()
        {
            GenResponse<IEnumerable<GenderCategory>> objResp = new() { Result = Array.Empty<GenderCategory>() };
            try
            {
                objResp.Result = await _cache.GetData<IEnumerable<GenderCategory>>($"{_appsettings.AppName}_AllGenderCategories");
                if (objResp.Result == null || !objResp.Result.Any())
                {
                    objResp.Result = _genericRepo.GetAll();
                    if (objResp.Result != null && objResp.Result.Any())
                    {
                        objResp.IsSuccess = true;
                        await _cache.SetData<IEnumerable<GenderCategory>>($"{_appsettings.AppName}_AllGenderCategories", objResp.Result, 60 * 60 * 12);
                    }
                    else
                    {
                        objResp.IsSuccess = false;
                        objResp.Message = $"No gender list details found.";
                    }
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<IEnumerable<GenderCategory>>(objResp, nameof(FetchAllGenderCategories), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public Task<GenResponse<GenderCategoryRespDTO>> FetchById(object id)
        {
            throw new NotImplementedException();
        }

        public Task<GenResponse<bool>> Remove(object Id)
        {
            throw new NotImplementedException();
        }

        public Task<GenResponse<bool>> Update(GenderCategoryUpdateRequestDTO model)
        {
            throw new NotImplementedException();
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
            };
            AppSessionData<AppUser> user = _appSession.GetUserDataFromSession();
            BackgroundJob.Enqueue(() => _activityLogRepo.LogToDatabase(log, user, caller));
        }
        #endregion

    }
}
