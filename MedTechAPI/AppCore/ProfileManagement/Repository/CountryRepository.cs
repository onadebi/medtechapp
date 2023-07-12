using AutoMapper;
using Common.Interface;
using Hangfire;
using MedTechAPI.AppCore.AppGlobal.Repository;
using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.DTO.LocationDTOs;
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
    public interface ICountryRepository
    {
        Task<GenResponse<List<CountryDetailsRespDTO>>> FetchAllCountries(bool includeDeletedAndInActive = false);
        Task<GenResponse<CountryDetailsRespDTO>> FetchCountryDetailById(int Id);
        Task<GenResponse<int>> AddNewCountryLocation(CountryCreationRequestDTO model);
        Task<GenResponse<bool>> UpdateCountryLocation(CountryUpdateRequestDTO model);
        Task<GenResponse<bool>> DeleteCountryLocation(int Id);
    }
    public class CountryRepository : ICountryRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CountryRepository> _logger;
        private readonly IAppActivityLogRepository _activityLogRepo;
        private readonly IAppSessionContextRepository _appSession;
        private readonly ICacheService _cache;
        private readonly AppSettings _appsettings;
        //private GenericRepository<CountryDetail> _genericRepo;
        private readonly IMapper _mapper;
        private readonly AppSessionData<AppUser> _appUser;
        public CountryRepository(AppDbContext context
            , ILogger<CountryRepository> logger
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
            _appsettings = appsettings.Value;
            _mapper = mapper;
            //if (_genericRepo == null)
            //{
            //    _genericRepo = new GenericRepository<CountryDetail>(_context);
            //}
            _appUser = _appSession.GetUserDataFromSession();
        }

        public async Task<GenResponse<int>> AddNewCountryLocation(CountryCreationRequestDTO model)
        {
            GenResponse<int> objResp = new();
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.CountryCode) || string.IsNullOrWhiteSpace(model.CountryName))
                {
                    return GenResponse<int>.Failed("Invalid data supplied");
                }
                model.CountryName = model.CountryName.CapitaliseFirstChar().Trim();
                model.CountryCode = model.CountryCode.Trim().ToUpper();
                CountryDetail countryDetail = _mapper.Map<CountryDetail>(model);
                countryDetail.ModifiedBy = string.IsNullOrWhiteSpace(_appUser.Data?.DisplayName) ? _appUser.SessionId : _appUser.Data?.DisplayName;
                countryDetail.UpdatedAt = DateTime.UtcNow;
                _context.CountryDetails.Add(countryDetail);
                objResp.IsSuccess = await _context.SaveChangesAsync() > 0;
                if (objResp.IsSuccess)
                {
                    objResp.Result = countryDetail.Id;
                    _ = _cache.RemoveData($"{_appsettings.AppName}_AllCountries");
                }
                else
                {
                    objResp.Message = objResp.Error = $"Failed to register [{model.CountryName}] country details.";
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
                LogRepositoryError<int>(objResp, nameof(AddNewCountryLocation), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<bool>> DeleteCountryLocation(int Id)
        {
            GenResponse<bool> objResp = new();
            if (Id <= 0)
            {
                return GenResponse<bool>.Failed($"No country exists for Id [{Id}]");
            }
            try
            {
                var result = await _context.CountryDetails.FirstOrDefaultAsync(m => m.Id == Id);
                if (result != null)
                {
                    result.UpdatedAt = DateTime.UtcNow;
                    result.ActiveStatus = false;
                    result.ModifiedBy = string.IsNullOrWhiteSpace(_appUser.Data?.DisplayName) ? _appUser.SessionId : _appUser.Data.DisplayName;
                    var objSave = await _context.SaveChangesAsync();

                    objResp.IsSuccess = objResp.Result = objSave > 0;
                    _ = _cache.RemoveData($"{_appsettings.AppName}_AllCountries");
                }
                else
                {
                    objResp.IsSuccess = objResp.Result = false;
                    objResp.Message = objResp.Error = $"No record ";
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<bool>(objResp, nameof(DeleteCountryLocation), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<List<CountryDetailsRespDTO>>> FetchAllCountries(bool includeInActive = false)
        {
            GenResponse<List<CountryDetailsRespDTO>> objResp = new() { Result = new() };
            try
            {
                if (!includeInActive)
                {
                    objResp.Result = await _cache.GetData<List<CountryDetailsRespDTO>>($"{_appsettings.AppName}_AllCountries");
                    objResp.IsSuccess = objResp.Result != null && objResp.Result.Any();
                }
                if (objResp.Result == null || !objResp.Result.Any())
                {
                    if (includeInActive == true)
                    {
                        objResp.Result = await _context.CountryDetails
                            .Select(m =>
                               new CountryDetailsRespDTO
                               {
                                   Id = m.Id,
                                   CountryCode = m.CountryCode,
                                   CountryName = m.CountryName
                               }).AsNoTracking().ToListAsync();
                    }
                    else
                    {
                        objResp.Result = await _context.CountryDetails.Where(m => m.ActiveStatus == true).Select(m =>
                            new CountryDetailsRespDTO
                            {
                                Id = m.Id,
                                CountryCode = m.CountryCode,
                                CountryName = m.CountryName
                            }).AsNoTracking().ToListAsync();
                    }
                    
                    if (objResp.Result != null && objResp.Result.Any())
                    {
                        objResp.IsSuccess = true;
                        if (!includeInActive)
                        {
                            await _cache.SetData<List<CountryDetailsRespDTO>>($"{_appsettings.AppName}_AllCountries", objResp.Result, 60 * 60 * 12);
                        }
                    }
                    else
                    {
                        objResp.IsSuccess = false;
                        objResp.Message = $"No country details found.";
                    }
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<List<CountryDetailsRespDTO>>(objResp, nameof(FetchAllCountries), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<CountryDetailsRespDTO>> FetchCountryDetailById(int Id)
        {
            GenResponse<CountryDetailsRespDTO> objResp = new();
            var objResult = await this.FetchAllCountries(includeInActive: false);
            if (objResult.IsSuccess && objResult.Result.Any())
            {
                objResp.Result = objResult.Result.FirstOrDefault(m => m.Id == Id);
                if (objResp.Result != null) { objResp.IsSuccess = true; }
            }
            else
            {
                objResp.IsSuccess = false;
                objResult.Message = objResp.Error = "No records found. Kindly retry.";
                objResp.StatCode = (int)StatusCodeEnum.NotFound;
            }
            return objResp;
        }

        public async Task<GenResponse<bool>> UpdateCountryLocation(CountryUpdateRequestDTO model)
        {
            GenResponse<bool> objResp = new();
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.CountryCode) || string.IsNullOrWhiteSpace(model.CountryName) || model.Id <= 0)
                {
                    return GenResponse<bool>.Failed("Invalid data supplied");
                }
                model.CountryCode = model.CountryCode.Trim().ToUpper();
                model.CountryName = model.CountryName.CapitaliseFirstChar().Trim();
                var countryDetailExists = await _context.CountryDetails.FirstOrDefaultAsync(m => m.Id == model.Id);
                if (countryDetailExists == null)
                {
                    return GenResponse<bool>.Failed($"No country detail with id {model.Id} found.");
                }
                countryDetailExists.CountryCode = model.CountryCode;
                countryDetailExists.CountryName = model.CountryName;
                countryDetailExists.ActiveStatus = model.IsActive;
                countryDetailExists.ModifiedBy = string.IsNullOrWhiteSpace(_appUser.Data?.DisplayName) ? _appUser.SessionId: _appUser.Data?.DisplayName;
                countryDetailExists.UpdatedAt = DateTime.UtcNow;
                //var  objSave = _context.CountryDetails.Update(countryDetailExists);
                objResp.Result = await _context.SaveChangesAsync() > 0;
                if (objResp.Result)
                {
                    objResp.Result = true;
                    objResp.Message = "Updated successfully";
                    _ = _cache.RemoveData($"{_appsettings.AppName}_AllCountries");
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<bool>(objResp, nameof(UpdateCountryLocation), ex, AppActivityOperationEnum.UserActivity);
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
