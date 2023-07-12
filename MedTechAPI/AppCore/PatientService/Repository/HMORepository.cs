using AutoMapper;
using Common.Interface;
using Hangfire;
using MedTechAPI.AppCore.AppGlobal.Repository;
using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using MedTechAPI.Domain.Enums;
using MedTechAPI.Persistence;
using Microsoft.AspNetCore.DataProtection.Repositories;
using OnaxTools.Dto.Http;
using OnaxTools.Dto.Identity;
using OnaxTools.Enums.Http;
using System.Runtime.CompilerServices;

namespace MedTechAPI.AppCore.PatientService.Repository
{
    public interface IHMORepository
    {

    }
    public class HMORepository : IHMORepository
    {
        private readonly ILogger<HMORepository> _logger;
        private readonly IAppActivityLogRepository _activityLogRepo;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly IAppSessionContextRepository _appSession;

        public HMORepository(ILogger<HMORepository> logger
            , IAppActivityLogRepository activityLogRepo
            , AppDbContext context
            , IMapper mapper
            , ICacheService cacheService
            , IAppSessionContextRepository appSession)
        {
            _logger = logger;
            _activityLogRepo = activityLogRepo;
            _context = context;
            _mapper = mapper;
            _cacheService = cacheService;
            _appSession = appSession;
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
