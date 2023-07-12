using AutoMapper;
using Common.Interface;
using Hangfire;
using MedTechAPI.AppCore.AppGlobal.Interface;
using MedTechAPI.AppCore.AppGlobal.Repository;
using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.AppCore.MedicCenter.Interface;
using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.DTO.MedicCompanyDTO;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using MedTechAPI.Domain.Enums;
using MedTechAPI.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnaxTools.Dto.Http;
using OnaxTools.Dto.Identity;
using OnaxTools.Enums.Http;
using System.Runtime.CompilerServices;

namespace MedTechAPI.AppCore.MedicCenter.Repository
{
    public class MedicBranchRepository : IMedicBranchRepository
    {
        private readonly ILogger<MedicBranchRepository> _logger;
        private readonly IAppActivityLogRepository _activityLogRepo;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;
        private readonly IAppSessionContextRepository _appSession;
        private readonly AppSettings _appsettings;
        private readonly IFilesUploadHelperService _uploadsHelperService;

        private string MedicBranchCache(string key) => $"{nameof(MedicBranchCache)}_{key}";
        private readonly AppSessionData<AppUser> _appUser;
        public MedicBranchRepository(ILogger<MedicBranchRepository> logger
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
            _appsettings = appsettings.Value;
            _uploadsHelperService = uploadsHelperService;

            _appUser = _appSession.GetUserDataFromSession();
        }


        #region MedicBranch

        public async Task<GenResponse<int>> Add(MedicBranchRegistrationDTO model)
        {
            GenResponse<int> objResp = new();
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.BranchName) || model.CompanyId <= 0)
                {
                    return GenResponse<int>.Failed("Invalid data supplied");
                }
                model.BranchName = model.BranchName.Trim();
                model.BranchAddress = model.BranchAddress.Trim();
                BranchDetail branchDetail = _mapper.Map<BranchDetail>(model);
                #region FileUpload
                if (model.LogoImage != null)
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
                        branchDetail.LogoPath = genResp.Result;
                    }
                }
                #endregion
                branchDetail.ModifiedBy = string.IsNullOrWhiteSpace(_appUser.Data?.DisplayName) ? _appUser.SessionId : _appUser.Data?.DisplayName;
                branchDetail.UpdatedAt = DateTime.UtcNow;
                _context.BranchDetails.Add(branchDetail);
                objResp.IsSuccess = await _context.SaveChangesAsync() > 0;
                if (objResp.IsSuccess)
                {
                    objResp.Result = branchDetail.Id;
                    _ = _cache.RemoveData($"{_appsettings.AppName}_AllBranches");
                }
                else
                {
                    objResp.Message = objResp.Error = $"Failed to register [{model.BranchName}] details.";
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
                LogRepositoryError<int>(objResp, nameof(Add), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }

        public async Task<GenResponse<List<MedicBranchDetailsDTO>>> FetchAllMedicBranchesByCompanyId(int Id, bool includeInActive = false)
        {
            GenResponse<List<MedicBranchDetailsDTO>> objResp = new() { Result = new() };
            try
            {
                if (!includeInActive)
                {
                    objResp.Result = await _cache.GetData<List<MedicBranchDetailsDTO>>($"{_appsettings.AppName}_AllBranches");
                    objResp.IsSuccess = objResp.Result != null && objResp.Result.Any();
                }
                if (objResp.Result == null || !objResp.Result.Any())
                {
                    if (includeInActive == true)
                    {
                        objResp.Result = await _context.BranchDetails.Where(m=> m.CompanyId == Id)
                            .Select(m =>
                               new MedicBranchDetailsDTO
                               {
                                   Id = m.Id,
                                   BranchName = m.BranchName,
                                   CountryId = m.CompanyDetail.CountryDetail.Id,
                                   CountryName = m.CompanyDetail.CountryDetail.CountryName,
                                   StateId = m.CompanyDetail.StateDetail.Id,
                                   StateName = m.CompanyDetail.StateDetail.StateName,
                                   BranchAddress = m.BranchAddress,
                                   LogoPath = m.LogoPath
                               }).AsNoTracking().ToListAsync();
                    }
                    else
                    {
                        objResp.Result = await _context.BranchDetails.Where(m => m.ActiveStatus == true && m.CompanyId == Id).Select(m =>
                            new MedicBranchDetailsDTO
                            {
                                Id = m.Id,
                                BranchName = m.BranchName,
                                CountryId = m.CompanyDetail.CountryDetail.Id,
                                CountryName = m.CompanyDetail.CountryDetail.CountryName,
                                StateId = m.CompanyDetail.StateDetail.Id,
                                StateName = m.CompanyDetail.StateDetail.StateName,
                                BranchAddress = m.BranchAddress,
                                LogoPath = m.LogoPath
                            }).AsNoTracking().ToListAsync();
                    }

                    if (objResp.Result != null && objResp.Result.Any())
                    {
                        objResp.IsSuccess = true;
                        if (!includeInActive)
                        {
                            await _cache.SetData<List<MedicBranchDetailsDTO>>($"{_appsettings.AppName}_AllBranches", objResp.Result, 60 * 60 * 12);
                        }
                    }
                    else
                    {
                        objResp.IsSuccess = false;
                        objResp.Message = $"No branch details found.";
                    }
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<List<MedicBranchDetailsDTO>>(objResp, nameof(FetchAllMedicBranchesByCompanyId), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;

        }


        public Task<GenResponse<bool>> Update(MedicBranchUpdateDTO model)
        {
            throw new NotImplementedException();
        }

        public async Task<GenResponse<List<MedicBranchDetailsDTO>>> FetchAll(bool includeInActive = false)
        {
            GenResponse<List<MedicBranchDetailsDTO>> objResp = new() { Result = new() };
            try
            {
                if (!includeInActive)
                {
                    objResp.Result = await _cache.GetData<List<MedicBranchDetailsDTO>>($"{_appsettings.AppName}_AllBranches");
                    objResp.IsSuccess = objResp.Result != null && objResp.Result.Any();
                }
                if (objResp.Result == null || !objResp.Result.Any())
                {
                    if (includeInActive == true)
                    {
                        objResp.Result = await _context.BranchDetails
                            .Select(m =>
                               new MedicBranchDetailsDTO
                               {
                                   Id = m.Id,
                                   BranchName = m.BranchName,
                                   CountryId = m.CompanyDetail.CountryDetail.Id,
                                   CountryName = m.CompanyDetail.CountryDetail.CountryName,
                                   StateId = m.CompanyDetail.StateDetail.Id,
                                   StateName = m.CompanyDetail.StateDetail.StateName,
                                   BranchAddress = m.BranchAddress,
                                   LogoPath = m.LogoPath,
                                   CompanyId = m.CompanyId,
                                   CompanyName = m.CompanyDetail.CompanyName
                               }).AsNoTracking().ToListAsync();
                    }
                    else
                    {
                        objResp.Result = await _context.BranchDetails.Where(m => m.ActiveStatus == true).Select(m =>
                            new MedicBranchDetailsDTO
                            {
                                Id = m.Id,
                                BranchName = m.BranchName,
                                CountryId = m.CompanyDetail.CountryDetail.Id,
                                CountryName = m.CompanyDetail.CountryDetail.CountryName,
                                StateId = m.CompanyDetail.StateDetail.Id,
                                StateName = m.CompanyDetail.StateDetail.StateName,
                                BranchAddress = m.BranchAddress,
                                LogoPath = m.LogoPath,
                                CompanyId = m.CompanyId,
                                CompanyName = m.CompanyDetail.CompanyName
                            }).AsNoTracking().ToListAsync();
                    }

                    if (objResp.Result != null && objResp.Result.Any())
                    {
                        objResp.IsSuccess = true;
                        if (!includeInActive)
                        {
                            await _cache.SetData<List<MedicBranchDetailsDTO>>($"{_appsettings.AppName}_AllBranches", objResp.Result, 60 * 60 * 12);
                        }
                    }
                    else
                    {
                        objResp.IsSuccess = false;
                        objResp.Message = $"No branch details found. Kindly retry.";
                    }
                }
            }
            catch (Exception ex)
            {
                LogRepositoryError<List<MedicBranchDetailsDTO>>(objResp, nameof(FetchAll), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;

        }

        public Task<GenResponse<MedicBranchDetailsDTO>> FetchById(object id)
        {
            throw new NotImplementedException();
        }

        public Task<GenResponse<bool>> Remove(object Id)
        {
            throw new NotImplementedException();
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
