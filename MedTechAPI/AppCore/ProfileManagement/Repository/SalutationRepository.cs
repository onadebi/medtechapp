using Common.Interface;
using Hangfire;
using MedTechAPI.AppCore.AppGlobal.Repository;
using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.DTO.Salutation;
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
    public interface ISalutationRepository
    {
        Task<GenResponse<IEnumerable<SalutationResponseDTO>>> FetchAllSalutations();
    }
    public class SalutationRepository : ISalutationRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SalutationRepository> _logger;
        private readonly IAppActivityLogRepository _activityLogRepo;
        private readonly IAppSessionContextRepository _appSession;
        private readonly ICacheService _cache;
        private readonly AppSettings _appsettings;
        private GenericRepository<Salutation> _genericRepo;
        public SalutationRepository(AppDbContext context
            , ILogger<SalutationRepository> logger
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
                _genericRepo = new GenericRepository<Salutation>(_context);
            }
        }

        public async Task<GenResponse<IEnumerable<SalutationResponseDTO>>> FetchAllSalutations()
        {
            GenResponse<IEnumerable<SalutationResponseDTO>> objResp = new() { Result = Array.Empty<SalutationResponseDTO>() };
            try
            {
                objResp.Result = await _cache.GetData<IEnumerable<SalutationResponseDTO>>($"{_appsettings.AppName}_FetchAllSalutations");
                if (objResp.Result == null || !objResp.Result.Any())
                {
                    //  _genericRepo.GetAll();
                    objResp.Result = await _context.Salutations.Where(m=> m.ActiveStatus ).Select(
                        m=> new SalutationResponseDTO{
                            Id = m.Id,
                            SalutationName = m.SalutationName
                        }
                    ).ToListAsync();
                    if (objResp.Result != null && objResp.Result.Any())
                    {
                        objResp.IsSuccess = true;
                        await _cache.SetData<IEnumerable<SalutationResponseDTO>>($"{_appsettings.AppName}_FetchAllSalutations", objResp.Result, 60 * 60 * 12);
                    }
                    else
                    {
                        objResp.IsSuccess = false;
                        objResp.Message = $"No Salutation details found.";
                    }
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<IEnumerable<SalutationResponseDTO>>(objResp, nameof(FetchAllSalutations), ex, AppActivityOperationEnum.UserActivity);
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
            };
            AppSessionData<AppUser> user = _appSession.GetUserDataFromSession();
            BackgroundJob.Enqueue(() => _activityLogRepo.LogToDatabase(log, user, caller));
        }
        #endregion
    }
}
