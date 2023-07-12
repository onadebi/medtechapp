using Hangfire;
using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using MedTechAPI.Domain.Enums;
using MedTechAPI.Persistence;
using OnaxTools.Dto.Identity;
using System.Runtime.CompilerServices;

namespace MedTechAPI.AppCore.AppGlobal.Repository
{
    public interface IAppActivityLogRepository
    {
        void LogToDatabase(AppActivityLog log, AppSessionData<AppUser> user, [CallerMemberName] string caller = "");
    }
    public class AppActivityLogRepository: IAppActivityLogRepository
    {
        private readonly ILogger<AppActivityLogRepository> _logger;
        private readonly AppDbContext _context;
        private readonly IAppSessionContextRepository _appSession;

        public AppActivityLogRepository(ILogger<AppActivityLogRepository> logger
            ,AppDbContext context
            , IAppSessionContextRepository appSession)
        {
            _logger = logger;
            _context = context;
            _appSession = appSession;
        }

        public void LogToDatabase(AppActivityLog log, AppSessionData<AppUser> user = null,  [CallerMemberName]string caller="")
        {
            if(log == null)
            {
                return;
            }
            try
            {
                user ??= _appSession.GetUserDataFromSession();
                log.MethodOperation = caller;
                log.UserGuid = user != null && user.Data != null ? user.Data.DisplayName : user.SessionId;
                _context.AppActivityLogs.Add(log);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return;
        }


    }
}
