using AutoMapper;
using Common.Interface;
using Hangfire;
using MedTechAPI.AppCore.AppGlobal.Repository;
using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.DTO.LocationDTOs;
using MedTechAPI.Domain.DTO.MedicCompanyDTO;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using MedTechAPI.Domain.Enums;
using MedTechAPI.Helpers;
using MedTechAPI.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnaxTools.Dto.Http;
using OnaxTools.Dto.Identity;
using OnaxTools.Enums.Http;
using System.Runtime.CompilerServices;

namespace MedTechAPI.AppCore.ProfileManagement.Repository
{
    public interface IStateLocationRepository
    {
        Task<GenResponse<IEnumerable<StateDetailsRespDTO>>> FetchAllStates(bool includeDeletedAndInActive = false);
        Task<GenResponse<StateDetailsRespDTO>> FetchStateById(int Id);
        Task<GenResponse<int>> AddNewStateLocation(StateCreationRequestDTO model);
        Task<GenResponse<bool>> UpdateStateLocation(UpdateStateDetailDTO model);
        Task<GenResponse<bool>> DeleteStateLocation(int Id);
    }
    public class StateLocationRepository : IStateLocationRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StateLocationRepository> _logger;
        private readonly IAppActivityLogRepository _activityLogRepo;
        private readonly IAppSessionContextRepository _appSession;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;
        private readonly AppSettings _appsettings;
        //private GenericRepository<StateDetail> _genericRepo;

        private readonly AppSessionData<AppUser> _appUser;
        public StateLocationRepository(AppDbContext context
            , ILogger<StateLocationRepository> logger
            , IAppActivityLogRepository activityLogRepo
            , IAppSessionContextRepository appSession
            , ICacheService cache
            , IOptions<AppSettings> appsettings
            , IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _activityLogRepo = activityLogRepo;
            _appSession = appSession;
            _cache = cache;
            _mapper = mapper;
            _appsettings = appsettings.Value;
            //if (_genericRepo == null)
            //{
            //    _genericRepo = new GenericRepository<StateDetail>(_context);
            //}
            _appUser = _appSession.GetUserDataFromSession();
        }

        public async Task<GenResponse<int>> AddNewStateLocation(StateCreationRequestDTO model)
        {
            GenResponse<int> objResp = new();
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.StateCode) || string.IsNullOrWhiteSpace(model.StateName))
                {
                    return GenResponse<int>.Failed("Invalid data supplied");
                }
                model.StateName = model.StateName.CapitaliseFirstChar().Trim();
                model.StateCode = model.StateCode.ToUpper().Trim();
                StateDetail stateDetail = _mapper.Map<StateDetail>(model);
                _context.StateDetails.Add(stateDetail);
                objResp.IsSuccess = await _context.SaveChangesAsync() > 0;
                if (objResp.IsSuccess)
                {
                    objResp.Result = stateDetail.Id;
                    _ = _cache.RemoveData($"{_appsettings.AppName}_FetchAllStates");
                }
                else
                {
                    objResp.Message = objResp.Error = $"Failed to add [{model.StateName}] state details.";
                    objResp.StatCode = (int)(StatusCodeEnum.NotModified);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.GetType() == typeof(Npgsql.PostgresException))
                    {
                        var except = ex.InnerException as Npgsql.PostgresException;
                        if (except != null && except.SqlState == "23505")
                        {
                            return GenResponse<int>.Failed($"Unable to add duplicate record(s).", StatusCodeEnum.NotImplemented);
                        }
                    }
                }
                LogRepositoryError<int>(objResp, nameof(AddNewStateLocation), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<bool>> DeleteStateLocation(int Id)
        {
            GenResponse<bool> objResp = new();
            if (Id <= 0)
            {
                return GenResponse<bool>.Failed($"No country exists for Id [{Id}]");
            }
            try
            {
                var result = await _context.StateDetails.FirstOrDefaultAsync(m => m.Id == Id);
                if (result != null)
                {
                    result.UpdatedAt = DateTime.UtcNow;
                    result.ActiveStatus = false;
                    result.ModifiedBy = string.IsNullOrWhiteSpace(_appUser.Data?.DisplayName) ? _appUser.SessionId : _appUser.Data.DisplayName;
                    var objSave = await _context.SaveChangesAsync();

                    objResp.IsSuccess = objResp.Result = objSave > 0;
                    _ = _cache.RemoveData($"{_appsettings.AppName}_FetchAllStates");
                }
                else
                {
                    objResp.IsSuccess = objResp.Result = false;
                    objResp.Message = objResp.Error = $"No record ";
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<bool>(objResp, nameof(DeleteStateLocation), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<IEnumerable<StateDetailsRespDTO>>> FetchAllStates(bool includeInActive = false)
        {
            GenResponse<IEnumerable<StateDetailsRespDTO>> objResp = new() { Result = Array.Empty<StateDetailsRespDTO>() };
            try
            {
                if (!includeInActive)
                {
                    objResp.Result = await _cache.GetData<IEnumerable<StateDetailsRespDTO>>($"{_appsettings.AppName}_FetchAllStates");
                    objResp.IsSuccess = objResp.Result != null && objResp.Result.Any();
                }
                if (objResp.Result == null || !objResp.Result.Any())
                {
                    if (includeInActive)
                    {
                        objResp.Result = await _context.StateDetails.Select(m => new StateDetailsRespDTO
                        {
                            Id = m.Id,
                            StateCode = m.StateCode,
                            StateName = m.StateName
                        }).ToListAsync();
                    }
                    else
                    {
                        objResp.Result = await _context.StateDetails.Where(m => m.ActiveStatus == true)
                        .Select(m => new StateDetailsRespDTO
                        {
                            Id = m.Id,
                            StateCode = m.StateCode,
                            StateName = m.StateName
                        }).ToListAsync();
                    }
                    if (objResp.Result != null && objResp.Result.Any())
                    {
                        objResp.IsSuccess = true;
                        if (!includeInActive)
                        {
                            await _cache.SetData<IEnumerable<StateDetailsRespDTO>>($"{_appsettings.AppName}_FetchAllStates", objResp.Result, 60 * 60 * 12);
                        }
                    }
                    else
                    {
                        objResp.IsSuccess = false;
                        objResp.Message = $"No state details found.";
                    }
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<IEnumerable<StateDetailsRespDTO>>(objResp, nameof(FetchAllStates), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<StateDetailsRespDTO>> FetchStateById(int Id)
        {
            GenResponse<StateDetailsRespDTO> objResp = new();
            var objResult = await FetchAllStates(includeInActive: false);
            if (objResult.IsSuccess && objResult.Result.Any())
            {
                objResp.Result = objResult.Result.FirstOrDefault(m => m.Id == Id);
                if (objResp.Result != null) { objResp.IsSuccess = true; }
                else
                {
                    objResp.StatCode = (int)StatusCodeEnum.NotFound;
                    objResp.Message = objResp.Error = $"No record found for state with Id [{Id}]";
                }
            }
            else
            {
                objResp.IsSuccess = false;
                objResult.Message = objResp.Error = "No records found. Kindly retry.";
                objResp.StatCode = (int)StatusCodeEnum.NotFound;
            }
            return objResp;
        }

        public async Task<GenResponse<bool>> UpdateStateLocation(UpdateStateDetailDTO model)
        {
            GenResponse<bool> objResp = new();
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.StateName) || string.IsNullOrWhiteSpace(model.StateCode) || model.CountryDetailId <= 0)
                {
                    return GenResponse<bool>.Failed("Invalid data supplied");
                }
                var stateDetailExists = await _context.StateDetails.FirstOrDefaultAsync(m => m.Id == model.Id);
                if (stateDetailExists == null)
                {
                    return GenResponse<bool>.Failed($"No state detail with id {model.Id} found.");
                }
                stateDetailExists.StateName = model.StateName.CapitaliseFirstChar().Trim();
                stateDetailExists.StateCode = model.StateCode.ToUpper().Trim();
                stateDetailExists.CountryDetailId = model.CountryDetailId;
                stateDetailExists.ActiveStatus = model.IsActive;
                stateDetailExists.ModifiedBy = string.IsNullOrWhiteSpace(_appUser.Data?.DisplayName) ? _appUser.SessionId : _appUser.Data?.DisplayName;
                stateDetailExists.UpdatedAt = DateTime.UtcNow;

                objResp.Result = await _context.SaveChangesAsync() > 0;
                if (objResp.IsSuccess)
                {
                    objResp.Result = true;
                    objResp.Message = "Updated successfully";
                    _ = _cache.RemoveData($"{_appsettings.AppName}_FetchAllStates");
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<bool>(objResp, nameof(UpdateStateLocation), ex, AppActivityOperationEnum.UserActivity);
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
