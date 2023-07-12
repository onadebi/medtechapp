using Common.DbAccess;
using Common.Interface;
using Dapper;
using Hangfire;
using MedTechAPI.AppCore.AppGlobal.Repository;
using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.DTO.Navigation;
using MedTechAPI.Domain.Entities.ProfileManagement;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using MedTechAPI.Domain.Enums;
using MedTechAPI.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OnaxTools.Dto.Http;
using OnaxTools.Dto.Identity;
using OnaxTools.Enums.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MedTechAPI.AppCore.Repository
{
    public class UserServiceRepository : IUserServiceRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserServiceRepository> _logger;
        private readonly ISqlDataAccess _sqlData;
        private readonly IMessageRepository _msgRepo;
        private readonly ICacheService _cacheService;
        private readonly IAppActivityLogRepository _activityLogRepo;
        private readonly IAppSessionContextRepository _appSession;
        private readonly AppSettings _appSettings;
        public UserServiceRepository(AppDbContext context, ILogger<UserServiceRepository> logger, ISqlDataAccess sqlData, IOptions<AppSettings> appSettings
            , IMessageRepository msgRepo
            , ICacheService cacheService
            , IAppActivityLogRepository activityLogRepo
            , IAppSessionContextRepository appSession
            )
        {
            _context = context;
            _logger = logger;
            _sqlData = sqlData;
            _msgRepo = msgRepo;
            _cacheService = cacheService;
            _activityLogRepo = activityLogRepo;
            _appSession = appSession;
            _appSettings = appSettings.Value;
        }


        public async Task<GenResponse<bool>> ResetForgottenPasswordRequest(string Email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(Email)) { return GenResponse<bool>.Failed("Invalid Email.", StatusCodeEnum.BadRequest); }
            try
            {
                Email = Email.Trim().ToLower();
                var emailExists = await _context.UserProfiles.FirstOrDefaultAsync(m => m.Email == Email, ct);
                if (emailExists == null) { return GenResponse<bool>.Failed("Email not found.", StatusCodeEnum.NotFound); }
                if (emailExists.IsDeleted || emailExists.IsDeactivated)
                {
                    return GenResponse<bool>.Failed("User account is currently deactivated or deleted.", StatusCodeEnum.Forbidden);
                }
                //TODO: COrrect below to use TokenBox table
                //var totalTokenPending = await _context.MessageBoxz.Where(t => ((t.ExpireAt - DateTime.UtcNow).Minutes >= 1) && (t.EmailReceiver == Email) && (t.IsUsed == false) && (t.TemplateId.Equals(MessageOperations.ResetPassword))).CountAsync();

                //if (totalTokenPending >= 5)
                //{
                //    return GenResponse<bool>.Failed("Excessive multiple attempts made. Kindly wait for 1 hour and try again.", StatusCodeEnum.BadRequest);
                //}

                #region Send Email
                string token = Guid.NewGuid().ToString();
                //string EmailBody = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"AppCore", "Onasonic", "Templates", "ForgottenPasswordRequest.html"))
                //    .Replace("##token##", token)
                //    .Replace("##email##", emailExists.Email)
                //    .Replace("##name##", emailExists.FirstName);
                //EmailModelWithDataDTO emailBody = new()
                //{
                //    ReceiverEmail = emailExists.Email,
                //    EmailSubject = MessageOperations.ResetPassword,
                //    EmailBody = EmailBody,
                //    EmailBodyData = new Dictionary<string, string>
                //    {
                //        { "##token##", token },
                //        { "##email##", token },
                //        { "##name##", token },
                //    }

                //};
                var msgBoxData = new Dictionary<string, string>
                    {
                        { "##token##", token },
                        { "##email##", emailExists.Email },
                        { "##name##", emailExists.FirstName },
                    };
                MessageBox msgBox = new MessageBox
                {
                    AppName = _appSettings.AppName,
                    EmailReceiver = emailExists.Email,
                    Operation = MessageOperations.ResetPassword,
                    UserId = emailExists.Email,
                    MessageData = JsonSerializer.Serialize(msgBoxData)
                };
                GenResponse<string> mailSendResult = await _msgRepo.InsertNewMessage(msgBox);
                if (!mailSendResult.IsSuccess)
                {
                    _logger.LogError($"Email failed to send for {msgBox.EmailReceiver} with token {token} for operation {msgBox.Operation}");
                }
                #endregion
                return mailSendResult.IsSuccess ? GenResponse<bool>.Success(true, StatusCodeEnum.OK, $"Instructions have been sent to your email on how to reset your passowrd.") : GenResponse<bool>.Failed($"Sorry, an internal error occured. Kindly try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UserServiceRepository][ResetForgottenPasswordRequest] {ex.Message}");
                return GenResponse<bool>.Failed($"Sorry, an internal error occured. Kindly try again.");
            }
        }


        public async Task<GenResponse<bool>> ResetForgottenPassword(UserAuthChangeForgottenPasswordDto user, CancellationToken ct = default)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Token))
            {
                return GenResponse<bool>.Failed("Invalid request");
            }
            if (user.NewPassword != user.ConfirmNewPassword)
            {
                return GenResponse<bool>.Failed("Passwords don't match.");
            }
            try
            {
                var userToken = new MessageBox();// await _context.MessageBoxz.FirstOrDefaultAsync(m => m.MessageData.Equals(user.Token.Trim()));

                //if (userToken == null || userToken.IsUsed || ((DateTime.UtcNow - userToken.ExpireAt).Minutes > 1))
                //{
                //    return GenResponse<bool>.Failed("Invalid request. Token is invalid, used or expired.");
                //}
                if (!userToken.EmailReceiver.Equals(user.Email.Trim(), StringComparison.CurrentCultureIgnoreCase))
                {
                    return GenResponse<bool>.Failed($"Invalid token for email {user.Email}.");
                }
                if (!userToken.Operation.Equals(MessageOperations.ResetPassword))
                {
                    return GenResponse<bool>.Failed("Invalid operation for user token.");
                }
                userToken.UpdatedAt = DateTime.UtcNow;
                var objRez = await _context.UserProfiles.FirstOrDefaultAsync(m => m.Email == user.Email.Trim().ToLower());
                if (objRez != null)
                {
                    objRez.Password = OnaxTools.Cryptify.EncryptSHA512(user.NewPassword);
                    objRez.DateLastUpdated = DateTime.UtcNow;
                }
                var objSave = await _context.SaveChangesAsync();
                if (objSave >= 1)
                {
                    return GenResponse<bool>.Success(true, StatusCodeEnum.OK, "Updated successfully.");
                }
                return GenResponse<bool>.Failed("Failed to update. Kindly try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UserServiceRepository][ResetForgottenPassword] {ex.Message}");
                return GenResponse<bool>.Failed($"Sorry, an internal error occured. Kindly try again.");
            }
        }
        public async Task<GenResponse<bool>> IsAnyUserProfileExist()
        {
            GenResponse<bool> objResp = new() { IsSuccess = true};
            try
            {
                objResp.Result  = await _context.UserProfiles.AnyAsync();                
            }
            catch (Exception ex)
            {
                LogRepositoryError<bool>(objResp, nameof(IsAnyUserProfileExist), ex, AppActivityOperationEnum.UserActivity);
            }
            return objResp;
        }
        public async Task<GenResponse<List<AppUser>>> GetAllUsers()
        {
            List<AppUser> allUsers = Enumerable.Empty<AppUser>().AsList();
            try
            {
                var cachedUsers = await _cacheService.GetData<List<AppUser>>("allUsers");
                if (cachedUsers != null && cachedUsers.Any())
                {
                    allUsers = cachedUsers;
                }
                else
                {
                    allUsers = await _context.UserProfiles.Include(m => m.UserProfileGroups).Select(u => new AppUser
                    {
                        DisplayName = $"{u.FirstName} {u.LastName}",
                        Email = u.Email,
                        Guid = Convert.ToString(u.Guid),
                        Roles = u.UserProfileGroups.Any() ? u.UserProfileGroups.Select(u => u.UserGroups.GroupName).ToList() : new List<string>()
                    }).ToListAsync();
                    if (allUsers != null && allUsers.Any())
                    {
                        await _cacheService.SetData<List<AppUser>>("allUsers", allUsers, 3 * 60);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UserServiceRepository][GetAllUsers] {ex.Message}");
                return GenResponse<List<AppUser>>.Failed($"Sorry, an internal error occured. Kindly try again.");
            }
            return allUsers != null ? GenResponse<List<AppUser>>.Success(allUsers) : GenResponse<List<AppUser>>.Failed("No record(s) found", StatusCodeEnum.NotFound);
        }

        public async Task<GenResponse<AppUser>> GetUserWithRolesByUserId(string UserGuid)
        {
            AppUser objResp = new AppUser();
            if (string.IsNullOrWhiteSpace(UserGuid))
            {
                return GenResponse<AppUser>.Failed("Invalid user details requested");
            }
            try
            {
                var cachedUser = await _cacheService.GetData<AppUser>($"UserWithRoles_{UserGuid}");
                if (cachedUser != null)
                {
                    objResp = cachedUser;
                }
                else
                {
                    objResp = await _context.UserProfiles.Include(m => m.UserProfileGroups)
                    .Select(u => new AppUser
                    {
                        DisplayName = $"{u.FirstName} {u.LastName}",
                        Email = u.Email,
                        Guid = u.Guid,
                        Roles = u.UserProfileGroups.Any() ? u.UserProfileGroups.Select(u => u.UserGroups.GroupName).ToList() : new List<string>()
                    }).FirstOrDefaultAsync(m => m.Guid == UserGuid);
                    if (objResp != null)
                    {
                        _ = _cacheService.SetData<AppUser>($"UserWithRoles_{UserGuid}", objResp, 60 * 5);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UserServiceRepository][GetUserWIthRoleByUserId] {ex.Message}");
                return GenResponse<AppUser>.Failed($"Sorry, an internal error occured. Kindly try again.");
            }
            return objResp == null ? GenResponse<AppUser>.Failed($"No profile found this user id.", StatusCodeEnum.NotFound) : GenResponse<AppUser>.Success(objResp);
        }

        public async Task<GenResponse<AppUser>> GetUserWithRolesByEmail(string Email)
        {
            AppUser objResp = new AppUser();
            if (string.IsNullOrWhiteSpace(Email))
            {
                return GenResponse<AppUser>.Failed("Invalid user details requested");
            }
            try
            {
                var cachedUser = await _cacheService.GetData<AppUser>($"UserWithRoles_{Email}");
                if (cachedUser != null)
                {
                    objResp = cachedUser;
                }
                else
                {
                    var objResult = await _context.UserProfiles.Include(m => m.UserProfileGroups).Select(u => new AppUser
                    {
                        DisplayName = $"{u.FirstName} {u.LastName}",
                        Email = u.Email,
                        Guid = Convert.ToString(u.Id),
                        Roles = u.UserProfileGroups.Any() ? u.UserProfileGroups.Select(u => new String(Convert.ToString(u.UserGroupId))).ToList() : new List<string>()
                    }).ToListAsync();
                    if (objResult != null && objResult.Any())
                    {
                        objResp = objResult.FirstOrDefault(m => m.Email == Email);
                    }

                    if (objResp != null)
                    {
                        await _cacheService.SetData<AppUser>($"UserWithRoles_{Email}", objResp, 60 * 10);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UserServiceRepository][GetUserWIthRoleByUserId] {ex.Message}");
                return GenResponse<AppUser>.Failed($"Sorry, an internal error occured. Kindly try again.");
            }
            return objResp != null ? GenResponse<AppUser>.Success(objResp) : GenResponse<AppUser>.Failed("No user record found.", StatusCodeEnum.NotFound);
        }

        public async Task<GenResponse<UserLoginResponse>> EmailRegistrationValidation(EmailValidationDto user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Token))
            {
                return GenResponse<UserLoginResponse>.Failed("Invalid request");
            }
            UserLoginResponse objResp = null;
            try
            {
                var userTokenOne = await _context.MessageBox.FirstOrDefaultAsync(m => m.MessageData.Equals(user.Token.Trim()) && m.UserId.Equals(user.UserGuid.Trim()));
                //new MessageBox(); // await _context.MessageBoxz.FirstOrDefaultAsync(m => m.MessageData.Equals(user.Token.Trim()));
                if (userTokenOne == null)
                {
                    return GenResponse<UserLoginResponse>.Failed("Invalid request. Token link to confirm email is invalid.");
                }
                if (((DateTime.UtcNow - userTokenOne.ExpiredAt).Minutes > 0) || userTokenOne.IsUsed || userTokenOne.CompletedStatus == 1)
                {
                    var userForEmailConfirmationResend = await _context.UserProfiles.FirstOrDefaultAsync(m => m.Guid == user.UserGuid.Trim());
                    if (userForEmailConfirmationResend == null)
                    {
                        return GenResponse<UserLoginResponse>.Failed("Invalid/Expired token on user account request.");
                    }
                    string confirmationToken = Guid.NewGuid().ToString();
                    string EmailBody = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCore", "Onasonic", "Templates", "ConfirmEmail.html"))
                            .Replace("##token##", confirmationToken)
                            .Replace("##name##", userForEmailConfirmationResend.FirstName)
                            .Replace("##guid##", userForEmailConfirmationResend.Guid);

                    EmailModelDTO emailBody = new EmailModelDTO()
                    {
                        ReceiverEmail = userForEmailConfirmationResend.Email,
                        EmailSubject = MessageOperations.ConfirmEmail,
                        EmailBody = EmailBody
                    };
                    MessageBox msg = new()
                    {
                        AppName = _appSettings.AppName,
                        Operation = AppConstants.EmailTemplateConfirmEmail,
                        Channel = "Email",
                        EmailReceiver = userForEmailConfirmationResend.Email,
                        IsProcessed = false,
                        IsUsed = false,
                        MessageData = confirmationToken,
                        UserId = userForEmailConfirmationResend.Email,
                        ForQueue = false,
                        ExpiredAt = DateTime.Now.AddMinutes(10)

                    };
                    GenResponse<string> mailSendResult = await _msgRepo.InsertNewMessageAndSendMail(emailBody, msg);
                    return GenResponse<UserLoginResponse>.Failed("Token link to confirm email is invalid, used or expired. Another link has been sent to your email for confirmation.");
                }
                //var userToken = userTokenOne.FirstOrDefault();
                //if (!userToken.EmailReceiver.Equals(userToken.EmailReceiver.Trim(), StringComparison.CurrentCultureIgnoreCase))
                //{
                //    return GenResponse<UserLoginResponse>.Failed($"Invalid token for email {userToken.EmailReceiver}.");
                //}

                //var builder = Builders<MessageBox>.Filter;
                //var filter = builder.Eq(x => x.Id, userTokenOne.Id)
                //    & builder.Eq(x => x.MessageData, user.Token);
                //_mongoDataAccess.GetCollection<MessageBox>().FindOneAndUpdate<MessageBox>(filter
                //    , Builders<MessageBox>.Update.Set(p => p.UpdatedAt, DateTime.UtcNow).Set(p => p.IsUsed, true));

                var objRez = await _context.UserProfiles.FirstOrDefaultAsync(m => m.Email == userTokenOne.EmailReceiver.Trim().ToLower());
                if (objRez != null)
                {
                    objRez.IsEmailConfirmed = true;
                    objRez.DateLastUpdated = DateTime.UtcNow;
                }
                userTokenOne.UpdatedAt = DateTime.UtcNow;
                userTokenOne.IsUsed = true;
                var objSave = await _context.SaveChangesAsync();
                //var objResult = await _sqlData.SaveData("UPDATE profile.\"UserProfile\" SET  \"DateLastUpdated\"= @updatedat , \"IsEmailConfirmed\"=true WHERE \"Email\" = @useremail;", new { updatedat = DateTime.UtcNow, useremail = userToken.EmailReceiver }, System.Data.CommandType.Text);
                if (objSave >= 1)
                {
                    //TODO: Fix below
                    //_= _mongoDataAccess.GetCollection<MessageBox>().FindOneAndReplace(Builders<MessageBox>.Filter.)
                    objResp = new UserLoginResponse
                    {
                        Email = objRez.Email,
                        FirstName = objRez.FirstName,
                        LastName = objRez.LastName,
                        token = null
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UserServiceRepository][EmailRegistrationValidation] {ex.Message}");
                return GenResponse<UserLoginResponse>.Failed("Unexpected error. Kindly try again", StatusCodeEnum.ServerError);
            }
            return objResp == null ? GenResponse<UserLoginResponse>.Failed("Operation failed. Kindly try again.") :
                GenResponse<UserLoginResponse>.Success(objResp, message: "Email has been sucessfully confirmed.");

        }

        public async Task<GenResponse<UserLoginResponse>> Login(UserLoginDto userLogin)
        {
            try
            {
                userLogin.Email = userLogin.Email.Trim().ToLower();
                var objResp = await _context.UserProfiles.FirstOrDefaultAsync(m => m.Email == userLogin.Email);
                //await _sqlData.GetData<UserLoginQueryResponseDto, dynamic>("select * from profile.loginuser(@useremail)", new { useremail = userLogin.Email });
                if (objResp != null)
                {
                    bool IsValidPwd = false;
                    IsValidPwd = OnaxTools.Cryptify.EncryptSHA512(userLogin.Password).Equals(objResp.Password);
                    if (!IsValidPwd)
                    {
                        return GenResponse<UserLoginResponse>.Failed("Invalid email/password supplied.", StatusCodeEnum.NotFound);
                    }
                    if (!objResp.IsEmailConfirmed)
                    {
                        string token = Guid.NewGuid().ToString();
                        var totalTokenPending = _context.MessageBox.Count(m=>( m.EmailReceiver == userLogin.Email.ToLower()) && (m.CompletedStatus == 1) && (m.Operation == MessageOperations.ConfirmEmail) && (m.ExpiredAt > DateTime.UtcNow));

                        if (totalTokenPending >= 5)
                        {
                            return GenResponse<UserLoginResponse>.Failed("Excessive multiple attempts made. Kindly wait for 24 hours and try again.");
                        }
                        string EmailBody = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCore", "Templates", "ConfirmEmail.html"))
                            .Replace("##token##", token)
                            .Replace("##guid##", objResp.Guid)
                            .Replace("##name##", objResp.FirstName);
                        EmailModelWithDataDTO emailBody = new()
                        {
                            ReceiverEmail = userLogin.Email,
                            EmailSubject = MessageOperations.ConfirmEmail,
                            EmailBody = EmailBody
                        };

                        MessageBox msgBox = new MessageBox
                        {
                            AppName = _appSettings.AppName,
                            EmailReceiver = objResp.Guid,
                            Operation = MessageOperations.ConfirmEmail,
                            MessageData = token,
                            UserId = objResp.Guid
                        };
                        GenResponse<string> mailSendResult = await _msgRepo.InsertNewMessageAndSendMail(emailBody, msgBox);
                        if (!mailSendResult.IsSuccess)
                        {
                            _logger.LogError($"Email failed to send for {msgBox.EmailReceiver} with token {token} for operation {msgBox.Operation}");
                        }
                        return GenResponse<UserLoginResponse>.Failed("Your email has not been confirmed. Check your email for confirmation mail to proceed.");
                    }
                    if (objResp.IsDeactivated || objResp.IsDeleted)
                    {
                        return GenResponse<UserLoginResponse>.Failed("User account is currently deactivated or deleted.", StatusCodeEnum.Forbidden);
                    }
                    else
                    {
                        int[] userRoleIds = _context.UserProfileGroups.Where(m => m.UserProfileId == objResp.Id).Select(m => m.UserGroupId).ToArray();
                        string[] userRoles = _context.UserGroup.Where(m => userRoleIds.Contains(m.Id)).Select(m => m.GroupName).ToArray();
                        UserLoginResponse result = new UserLoginResponse()
                        {
                            Email = objResp.Email,
                            FirstName = objResp.FirstName,
                            LastName = objResp.LastName,
                            Roles = userRoles,
                            Guid = objResp.Guid,
                            Id = objResp.Id,
                            CompanyId = objResp.MedicCompanyId
                        };
                        return GenResponse<UserLoginResponse>.Success(result);
                    }
                }
                else
                {
                    return GenResponse<UserLoginResponse>.Failed("User account not found or has been deleted.", StatusCodeEnum.NotFound);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"[UserServiceRepository][Login] {ex.Message}");
                return GenResponse<UserLoginResponse>.Failed("Bad request");
            }
        }
        public async Task<GenResponse<UserLoginResponse>> RegisterAdmin(UserModelCreateDto user)
        {
            if (user == null)
            {
                return GenResponse<UserLoginResponse>.Failed("Invalid parameters passed");
            }
            if (user.Password != user.ConfirmPassword)
            {
                return GenResponse<UserLoginResponse>.Failed("Passwords don't match.");
            }
            user.Email = user.Email.ToLower();
            var isExist = await _context.UserProfiles.FirstOrDefaultAsync(m => m.Email == user.Email);
            if (isExist != null)
            {
                return GenResponse<UserLoginResponse>.Failed("Another user registered with this email already exists.");
            }
            var anyExist = await _context.UserProfiles.AnyAsync();
            if (anyExist)
            {
                return GenResponse<UserLoginResponse>.Failed("Only a first registered user can be defaulted to admin priviledges. Contact admin/support for ryour registration.");
            }

            var parameters = new UserProfile
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                MedicBranchId = user.MedicBranchId,
                MedicCompanyId = user.MedicCompanyId,
                SalutationId = user.SalutationId,
                GenderCategoryId = user.GenderCategoryId,
                Password = OnaxTools.Cryptify.EncryptSHA512(user.Password)
            };
            UserLoginResponse objResp = null;
            try
            {
                var userGroups = await _context.UserGroup.ToListAsync();
                if (userGroups == null || userGroups.Count == 0)
                {
                    return GenResponse<UserLoginResponse>.Failed("Invalid usergroup selection.");
                }
                userGroups.ForEach(m => parameters.UserProfileGroups.Add(new UserProfileGroup { UserGroupId = m.Id }));

                var regUser = await _context.UserProfiles.AddAsync(parameters);

                var objSave = await _context.SaveChangesAsync();
                if (objSave > 0)
                {
                    objResp = new UserLoginResponse()
                    {
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Roles = userGroups.Select(m => m.GroupName).ToArray<string>(),
                        Guid = parameters.Guid
                    };
                    string confirmationToken = Guid.NewGuid().ToString();
                    string EmailBody = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCore", "Templates", "ConfirmEmail.html"))
                            .Replace("##token##", confirmationToken)
                            .Replace("##name##", user.FirstName)
                            .Replace("##guid##", parameters.Guid);
                    EmailModelDTO emailBody = new EmailModelDTO()
                    {
                        ReceiverEmail = user.Email,
                        EmailSubject = MessageOperations.ConfirmEmail,
                        EmailBody = EmailBody
                    };
                    MessageBox msg = new()
                    {
                        AppName = _appSettings.AppName,
                        Operation = AppConstants.EmailTemplateConfirmEmail,
                        Channel = "Email",
                        EmailReceiver = user.Email,
                        IsProcessed = false,
                        IsUsed = false,
                        MessageData = confirmationToken,
                        UserId = parameters.Guid,
                        ForQueue = false,
                        ExpiredAt = DateTime.Now.AddMinutes(10)
                    };
                    GenResponse<string> mailSendResult = await _msgRepo.InsertNewMessageAndSendMail(emailBody, msg);
                }
                else
                {
                    return GenResponse<UserLoginResponse>.Failed("Unable to complete. Kindly try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return GenResponse<UserLoginResponse>.Failed("An internal error occured. Kindly try again.");
            }
            return GenResponse<UserLoginResponse>.Success(objResp, StatusCodeEnum.Created, "Your email has not been confirmed. Check your email for confirmation mail to proceed.");
        }
        public async Task<GenResponse<UserLoginResponse>> RegisterUser(UserModelCreateDto user)
        {
            if (user == null)
            {
                return GenResponse<UserLoginResponse>.Failed("Invalid parameters passed");
            }
            if (user.Password != user.ConfirmPassword)
            {
                return GenResponse<UserLoginResponse>.Failed("Passwords don't match.");
            }
            user.Email = user.Email.ToLower();
            var isExist = await _context.UserProfiles.FirstOrDefaultAsync(m => m.Email == user.Email);
            if (isExist != null)
            {
                return GenResponse<UserLoginResponse>.Failed("Another user registered with this email already exists.");
            }

            var parameters = new UserProfile
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                MedicBranchId = user.MedicBranchId,
                MedicCompanyId = user.MedicCompanyId,
                SalutationId = user.SalutationId,
                GenderCategoryId = user.GenderCategoryId,
                Password = OnaxTools.Cryptify.EncryptSHA512(user.Password)
            };
            UserLoginResponse objResp = null;
            try
            {
                var userGroups = await _context.UserGroup.Where(m=> user.UserGroupIds.Contains(m.Id)).ToListAsync();
                if(userGroups == null || userGroups.Count == 0){
                    return GenResponse<UserLoginResponse>.Failed("Invalid usergroup selection.");
                }
                userGroups.ForEach(m=> parameters.UserProfileGroups.Add(new UserProfileGroup{UserGroupId = m.Id  }));

                var regUser = await _context.UserProfiles.AddAsync(parameters);

                var objSave = await _context.SaveChangesAsync();
                if (objSave > 0)
                {
                    // int[] userRoleIds = parameters.UserProfileGroups.Select(m => m.UserGroupId).ToArray();
                    // string[] userRoles = _context.UserGroup.Where(m => userRoleIds.Contains(m.Id)).Select(m => m.GroupName).ToArray();
                    objResp = new UserLoginResponse()
                    {
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Roles = userGroups.Select(m=> m.GroupName).ToArray<string>(),
                        Guid = parameters.Guid
                    };
                    string confirmationToken = Guid.NewGuid().ToString();
                    string EmailBody = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCore", "Templates", "ConfirmEmail.html"))
                            .Replace("##token##", confirmationToken)
                            .Replace("##name##", user.FirstName)
                            .Replace("##guid##", parameters.Guid);
                    EmailModelDTO emailBody = new EmailModelDTO()
                    {
                        ReceiverEmail = user.Email,
                        EmailSubject = MessageOperations.ConfirmEmail,
                        EmailBody = EmailBody
                    };
                    MessageBox msg = new()
                    {
                        AppName = _appSettings.AppName,
                        Operation = AppConstants.EmailTemplateConfirmEmail,
                        Channel = "Email",
                        EmailReceiver = user.Email,
                        IsProcessed = false,
                        IsUsed = false,
                        MessageData = confirmationToken,
                        UserId = parameters.Guid,
                        ForQueue = false,
                        ExpiredAt = DateTime.Now.AddMinutes(10)

                    };
                    GenResponse<string> mailSendResult = await _msgRepo.InsertNewMessageAndSendMail(emailBody, msg);
                }
                else
                {
                    return GenResponse<UserLoginResponse>.Failed("Unable to complete. Kindly try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return GenResponse<UserLoginResponse>.Failed("An internal error occured. Kindly try again.");
            }
            return GenResponse<UserLoginResponse>.Success(objResp, StatusCodeEnum.Created, "Your email has not been confirmed. Check your email for confirmation mail to proceed.");
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
        #endregion

    }
}
