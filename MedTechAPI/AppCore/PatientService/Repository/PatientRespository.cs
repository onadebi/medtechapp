using AutoMapper;
using Common.Interface;
using Hangfire;
using MedTechAPI.AppCore.AppGlobal.Repository;
using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.AppCore.PatientService.Interface;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.DTO.PatientDTOs;
using MedTechAPI.Domain.Entities.PatientEntitites;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using MedTechAPI.Domain.Enums;
using MedTechAPI.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using OnaxTools.Dto.Http;
using OnaxTools.Dto.Identity;
using OnaxTools.Enums.Http;
using System.Runtime.CompilerServices;

namespace MedTechAPI.AppCore.PatientService.Repository
{

    public class PatientRespository : IPatientRespository
    {
        private readonly ILogger<PatientRespository> _logger;
        private readonly IAppActivityLogRepository _activityLogRepo;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly IAppSessionContextRepository _appSession;

        private string PatientCache(string key) => $"{nameof(PatientCache)}_{key}";

        public PatientRespository(ILogger<PatientRespository> logger
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

        public async Task<GenResponse<List<PatientCategoryRespDTO>>> FetchAllPatientCategories()
        {
            GenResponse<List<PatientCategoryRespDTO>> objResp = new() { Result = new() };
            try
            {
                objResp.Result = await _cacheService.GetData<List<PatientCategoryRespDTO>>(PatientCache("AllPatientCategories"));
                if (objResp.Result == null || !objResp.Result.Any())
                {
                    var allPatientCategories = await _context.PatientCategory.Where(m=> m.ActiveStatus).AsNoTracking().ToListAsync();
                    if (allPatientCategories.Any())
                    {
                        objResp.Result = _mapper.Map<List<PatientCategoryRespDTO>>(allPatientCategories);
                        objResp.IsSuccess = true;
                        await _cacheService.SetData(PatientCache("AllPatientCategories"), objResp.Result, 60 * 60);
                    }
                    else
                    {
                        objResp.IsSuccess = false;
                        objResp.StatCode = (int)StatusCodeEnum.NotFound;
                    }
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<List<PatientCategoryRespDTO>>(objResp, nameof(FetchAllPatientCategories), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<int>> AddNewPatientCategory(PatientCategoryCreationDTO model)
        {
            GenResponse<int> objResp = new();
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.CategoryName))
                {
                    objResp.Error = objResp.Message = "Invalid entries passed.";
                    objResp.StatCode = (int)StatusCodeEnum.BadRequest;
                    return objResp;
                }
                model.CategoryName = model.CategoryName.Trim();
                var newPatientCategory = _mapper.Map<PatientCategory>(model);
                _context.PatientCategory.Add(newPatientCategory);
                await _context.SaveChangesAsync();
                objResp.IsSuccess = newPatientCategory.Id > 0;
                if (objResp.IsSuccess)
                {
                    objResp.Result = newPatientCategory.Id;
                    _ = _cacheService.RemoveData(PatientCache("AllPatientCategories"));
                }
                else
                {
                    objResp.StatCode = (int)StatusCodeEnum.NotModified;
                    objResp.Error = objResp.Message = "Failed to add new record. Kindly retry.";
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
                LogRepositoryError<int>(objResp, nameof(AddNewPatientCategory), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<string>> RegisterNewPatient(RegisterNewPatientDto model)
        {
            GenResponse<string> objResp = new();
            try
            {
                if (model == null)
                {
                    return GenResponse<string>.Failed("Invalid data supplied");
                }
                PatientProfile patient = _mapper.Map<PatientProfile>(model);
                patient.PatientCode = $"{patient.Id}-HosPiName-BranchCode";
                _context.PatientProfiles.Add(patient);
                int objSaveResult = await _context.SaveChangesAsync();
                if (objSaveResult > 0)
                {
                    objResp.Result = patient.PatientCode;
                    objResp.IsSuccess = false;
                }
                else
                {
                    objResp.Message = objResp.Error = $"Failed to save [{model.FirstName} {model.LastName}] patient details";
                    objResp.StatCode = (int)(StatusCodeEnum.NotModified);
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<string>(objResp, nameof(RegisterNewPatient), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }


        public async Task<GenResponse<bool>> UpdatePatientCategory(PatientCategoryUpdateDTO model)
        {
            GenResponse<bool> objResp = new();
            try
            {
                if (string.IsNullOrWhiteSpace(model.CategoryName) || model.Id <= 0)
                {
                    objResp.Error = objResp.Message = "Invalid entries passed.";
                    objResp.StatCode = (int)StatusCodeEnum.BadRequest;
                    return objResp;
                }
                var categoryExists = await _context.PatientCategory.FirstOrDefaultAsync(m=> m.Id == model.Id);
                if (categoryExists == null)
                {
                    return GenResponse<bool>.Failed($"No patient category with id {model.Id} found.");
                }                
                categoryExists.CategoryName = model.CategoryName;
                categoryExists.ActiveStatus = model.IsActive;

                objResp.Result = await _context.SaveChangesAsync() > 0;
                if (objResp.IsSuccess)
                {
                    objResp.Result = true;
                    objResp.Message = "Updated successfully";
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<bool>(objResp, nameof(UpdatePatientCategory), ex, AppActivityOperationEnum.UserActivity);
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
