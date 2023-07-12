using AutoMapper;
using Common.Interface;
using Hangfire;
using MedTechAPI.AppCore.AppGlobal.Repository;
using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.AppCore.MedicCenter.Interface;
using MedTechAPI.AppCore.PatientService.Repository;
using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.DTO.PatientDTOs;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.Entities.PatientEntitites;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using MedTechAPI.Domain.Enums;
using MedTechAPI.Persistence;
using Microsoft.Extensions.Options;
using OnaxTools.Dto.Http;
using OnaxTools.Dto.Identity;
using OnaxTools.Enums.Http;
using System.Runtime.CompilerServices;
using MedTechAPI.Domain.DTO.MedicCompanyDTO;
using MedTechAPI.AppCore.AppGlobal.Interface;
using Microsoft.EntityFrameworkCore;

namespace MedTechAPI.AppCore.MedicCenter.Repository
{
    public class MedicCompanyRepository : IMedicCompanyRepository
    {
        private readonly ILogger<MedicCompanyRepository> _logger;
        private readonly IAppActivityLogRepository _activityLogRepo;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;
        private readonly IAppSessionContextRepository _appSession;
        private readonly IFilesUploadHelperService _uploadsHelperService;
        private readonly AppSettings _appsettings;

        private string MedicCompanyCache(string key) => $"{nameof(MedicCompanyCache)}_{key}";
        private readonly AppSessionData<AppUser> _appUser;
        public MedicCompanyRepository(ILogger<MedicCompanyRepository> logger
            , IAppActivityLogRepository activityLogRepo
            , AppDbContext context
            , IMapper mapper
            , ICacheService cacheService
            , IAppSessionContextRepository appSession
            , IOptions<AppSettings> appsettings
            , IFilesUploadHelperService uploadsHelperService)
        {
            _logger = logger;
            _activityLogRepo = activityLogRepo;
            _context = context;
            _mapper = mapper;
            _cache = cacheService;
            _appSession = appSession;
            _uploadsHelperService = uploadsHelperService;
            _appsettings = appsettings.Value;

            _appUser = _appSession.GetUserDataFromSession();
        }

        #region MedicCompany
        public async Task<GenResponse<List<MedicCompanyDetailsDTO>>> FetchAllMedicCompanyDetails()
        {
            GenResponse<List<MedicCompanyDetailsDTO>> objResp = new() { Result = new() };
            try
            {
                objResp.Result = await _cache.GetData<List<MedicCompanyDetailsDTO>>(MedicCompanyCache("AllMedicCompanyDetails"));
                if (objResp.Result == null || !objResp.Result.Any())
                {
                    var allMedicCompanyDetails = await _context.MedicCompanyDetails.Where(m => m.ActiveStatus).Select(m => new MedicCompanyDetailsDTO
                    {
                        Id = m.Id,
                        CompanyName = m.CompanyName,
                        CompanyAddress = m.CompanyAddress,
                        CountryId = m.CountryId,
                        CountryName = m.CountryDetail.CountryName,
                        StateId = m.StateId,
                        StateName = m.StateDetail.StateName,
                        LogoPath = m.LogoPath
                    }).AsNoTracking().ToListAsync();
                    if (allMedicCompanyDetails.Any())
                    {
                        objResp.Result = allMedicCompanyDetails;
                        objResp.IsSuccess = true;
                        await _cache.SetData(MedicCompanyCache("AllMedicCompanyDetails"), objResp.Result, 60 * 60 * 12);
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
                LogRepositoryError<List<MedicCompanyDetailsDTO>>(objResp, nameof(FetchAllMedicCompanyDetails), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<MedicCompanyDetailsDTO>> FetchMedicCompanyDetailsById(int id)
        {
            GenResponse<MedicCompanyDetailsDTO> objResp = new();
            try
            {
                var allCompany = await this.FetchAllMedicCompanyDetails();
                if (allCompany.IsSuccess && allCompany.Result.Any())
                {
                    objResp.Result = allCompany.Result.FirstOrDefault(m => m.Id == id);
                    if (objResp.Result != null)
                    {
                        objResp.IsSuccess = true;
                    }
                    else
                    {
                        objResp.Message = objResp.Error = $"No record found for company with Id [{id}]";
                        objResp.StatCode = (int)StatusCodeEnum.NotFound;
                    }
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<MedicCompanyDetailsDTO>(objResp, nameof(FetchMedicCompanyDetailsById), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }
        public async Task<GenResponse<int>> RegisterNewMedicCompany(MedicCompanyRegistrationDTO model)
        {
            GenResponse<int> objResp = new();
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.CompanyName))
                {
                    return GenResponse<int>.Failed("Invalid data supplied");
                }
                model.CompanyName = model.CompanyName.Trim();
                MedicCompanyDetail medicCompany = _mapper.Map<MedicCompanyDetail>(model);


                _context.MedicCompanyDetails.Add(medicCompany);


                objResp.IsSuccess = await _context.SaveChangesAsync() > 0;
                #region FileUpload
                if (model.LogoImage != null && objResp.IsSuccess)
                {
                    var allowedFileTypes = Enum.GetValues(typeof(AllowedImageFileUploadsEnum)).Cast<AllowedImageFileUploadsEnum>().ToList();
                    string fileExtension = System.IO.Path.GetExtension(model.LogoImage.FileName);
                    Enum.TryParse<AllowedImageFileUploadsEnum>(fileExtension, ignoreCase: true, out AllowedImageFileUploadsEnum _result);
                    if (!allowedFileTypes.Contains(_result))
                    {
                        return GenResponse<int>.Failed("Invalid image format uploaded.");
                    }
                    string fileName = $"{Guid.NewGuid()}_{model.LogoImage.FileName}";
                    GenResponse<string> genResp = await _uploadsHelperService.UploadSingleImageFileToPath(model.LogoImage, fileName);
                    if (genResp.IsSuccess)
                    {
                        medicCompany.LogoPath = genResp.Result;
                    }
                }
                #endregion
                if (objResp.IsSuccess)
                {
                    objResp.Result = medicCompany.Id;

                    #region Head branch
                    BranchDetail branch = new();// _mapper.Map<BranchDetail>(model);
                    if (model.IsHeadBranch)
                    {
                        branch = new BranchDetail
                        {
                            BranchName = model.CompanyName,
                            CountryId = model.CountryId,
                            StateId = model.StateId,
                            BranchAddress = model.CompanyAddress,
                            CompanyId = medicCompany.Id,
                            LogoPath = medicCompany.LogoPath
                        };
                        _context.BranchDetails.Add(branch);
                        var branchSave = await _context.SaveChangesAsync();
                        objResp.Message = branchSave > 0 ? "Head branch created successfully": null;
                    }
                    #endregion

                    _ = _cache.RemoveData(MedicCompanyCache("AllMedicCompanyDetails"));
                }
                else
                {
                    objResp.Message = objResp.Error = $"Failed to register [{model.CompanyName}] company details.";
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
                LogRepositoryError<int>(objResp, nameof(RegisterNewMedicCompany), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<bool>> UpdateMedicCompanyDetails(MedicCompanyUpdateDTO model)
        {
            GenResponse<bool> objResp = new();
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.CompanyName))
                {
                    return GenResponse<bool>.Failed("Invalid data supplied");
                }
                var medicCompanyExits = await _context.MedicCompanyDetails.FirstOrDefaultAsync(m => m.Id == model.Id);
                if (medicCompanyExits == null)
                {
                    return GenResponse<bool>.Failed($"No company detail with id {model.Id} found.");
                }
                medicCompanyExits = _mapper.Map<MedicCompanyDetail>(model);
                //medicCompanyExits.CompanyName = model.CompanyName;
                //medicCompanyExits.CompanyAddress = model.CompanyAddress;
                #region FileUpload
                if (model.LogoImage != null)
                {
                    var allowedFileTypes = Enum.GetValues(typeof(AllowedImageFileUploadsEnum)).Cast<AllowedImageFileUploadsEnum>().ToList();
                    string fileExtension = System.IO.Path.GetExtension(model.LogoImage.FileName);
                    Enum.TryParse<AllowedImageFileUploadsEnum>(fileExtension, ignoreCase: true, out AllowedImageFileUploadsEnum _result);
                    if (!allowedFileTypes.Contains(_result))
                    {
                        return GenResponse<bool>.Failed("Invalid image format uploaded.");
                    }
                    string fileName = $"{Guid.NewGuid()}_{model.LogoImage.FileName}";
                    GenResponse<string> genResp = await _uploadsHelperService.UploadSingleImageFileToPath(model.LogoImage, fileName);
                    if (genResp.IsSuccess)
                    {
                        medicCompanyExits.LogoPath = genResp.Result;
                    }
                }
                #endregion
                objResp.Result = await _context.SaveChangesAsync() > 0;
                if (objResp.IsSuccess)
                {
                    objResp.Result = true;
                    objResp.Message = "Updated successfully";
                    _ = _cache.RemoveData(MedicCompanyCache("AllMedicCompanyDetails"));
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<bool>(objResp, nameof(UpdateMedicCompanyDetails), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<bool>> Remove(int Id)
        {
            GenResponse<bool> objResp = new();
            if (Id <= 0)
            {
                return GenResponse<bool>.Failed($"No company exists for Id [{Id}]");
            }
            try
            {
                var result = await _context.MedicCompanyDetails.FirstOrDefaultAsync(m => m.Id == Id);
                if (result != null)
                {
                    result.UpdatedAt = DateTime.UtcNow;
                    result.ActiveStatus = false;
                    result.ModifiedBy = string.IsNullOrWhiteSpace(_appUser.Data?.DisplayName) ? _appUser.SessionId : _appUser.Data.DisplayName;
                    var objSave = await _context.SaveChangesAsync();

                    objResp.IsSuccess = objResp.Result = objSave > 0;
                    _ = _cache.RemoveData(MedicCompanyCache("AllMedicCompanyDetails"));
                }
                else
                {
                    objResp.IsSuccess = objResp.Result = false;
                    objResp.Message = objResp.Error = $"No record ";
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<bool>(objResp, nameof(Remove), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        #endregion


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
