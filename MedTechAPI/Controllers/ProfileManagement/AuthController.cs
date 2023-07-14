using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.Controllers.Setup;
using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.Enums;
using MedTechAPI.Extensions.Middleware;
using MedTechAPI.Extensions.Services;
using MedTechAPI.Helpers.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnaxTools.Dto.Http;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace MedTechAPI.Controllers.ProfileManagement
{
    public class AuthController : BaseSetupController
    {
        private readonly TokenService _tokenService;
        private readonly IUserServiceRepository _userService;
        private readonly IUserGroupRepository _userRoleService;
        private readonly IAppSessionContextRepository _sessionContextRepo;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly SessionConfig _sessionConfig;

        public AuthController(TokenService tokenService, IUserServiceRepository userService
        , IUserGroupRepository userRoleService
        , IAppSessionContextRepository sessionContextRepo
        ,IHttpContextAccessor contextAccessor
            , IOptions<SessionConfig> sessionConfig)
        {
            _tokenService = tokenService;
            _userService = userService;
            _userRoleService = userRoleService;
            _sessionContextRepo = sessionContextRepo;
            _contextAccessor = contextAccessor;
            _sessionConfig = sessionConfig.Value;
        }

        [HttpGet(nameof(Me))]
        //[Auth("conTributor", "Them")]
        [ProducesResponseType(typeof(GenResponse<AppUser>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Me()
        {
            var objResp = await Task.Run(() => _sessionContextRepo.GetUserDataFromSession());
            return Ok(objResp.Data);
        }

        [AllowAnonymous]
        [HttpGet(nameof(AnyUserProfileExists))]
        //[Auth("conTributor", "Them")]
        [ProducesResponseType(typeof(GenResponse<AppUser>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(Summary = "This is to be used as a one-off action to check if any userprofile has been onboarded.", Description = "This informaion is useful if there is a need to onboard the very first user with default highest level group rights.")]
        public async Task<IActionResult> AnyUserProfileExists()
        {
            var objResp = await Task.Run(() => _userService.IsAnyUserProfileExist());
            return Ok(objResp.Result);
        }

        [AllowAnonymous]
        [HttpGet(nameof(ResetForgottenPasswordRequest))]
        [ProducesResponseType(typeof(GenResponse<bool>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ResetForgottenPasswordRequest([Required] string Email)
        {
            var objResp = await _userService.ResetForgottenPasswordRequest(Email);
            return Ok(objResp);
        }

        [AllowAnonymous]
        [HttpPut(nameof(ResetForgottenPassword))]
        [ProducesResponseType(typeof(GenResponse<bool>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ResetForgottenPassword(UserAuthChangeForgottenPasswordDto user)
        {
            var objResp = await _userService.ResetForgottenPassword(user);
            return StatusCode(objResp.StatCode, objResp);
        }

        [HttpGet(nameof(GetAllUsers))]
        [Auth(nameof(UserRolesEnum.Admin))]
        [ProducesResponseType(typeof(GenResponse<List<AppUser>>), 200)]
        public async Task<IActionResult> GetAllUsers()
        {
            var objResp = await _userService.GetAllUsers();
            return Ok(objResp);
        }

        [AllowAnonymous]
        [HttpPost(nameof(RegisterAdmin))]
        [ProducesResponseType(typeof(GenResponse<UserLoginResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RegisterAdmin(UserModelCreateDto user)
        {
            var objResp = await _userService.RegisterAdmin(user);
            if (objResp.IsSuccess && !string.IsNullOrWhiteSpace(objResp.Result.Email))
            {
                objResp.Result.token = _tokenService.CreateToken(new AppUser() { DisplayName = $"{objResp.Result.FirstName} {objResp.Result.LastName}", Email = objResp.Result.Email, Guid = objResp.Result.Guid, Roles = objResp.Result.Roles.ToList<string>() }, _sessionConfig.Auth.ExpireMinutes);
                return Ok(objResp);
            }
            return Ok(objResp); ;
        }

        [HttpPost(nameof(Register))]
        [ProducesResponseType(typeof(GenResponse<UserLoginResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Register(UserModelCreateDto user)
        {
            var objResp = await _userService.RegisterUser(user);
            if (objResp.IsSuccess && !string.IsNullOrWhiteSpace(objResp.Result.Email))
            {
                objResp.Result.token = _tokenService.CreateToken(new AppUser() { DisplayName = $"{objResp.Result.FirstName} {objResp.Result.LastName}", Email = objResp.Result.Email, Guid = objResp.Result.Guid, Roles = objResp.Result.Roles.ToList<string>() }, _sessionConfig.Auth.ExpireMinutes);
                return Ok(objResp);
            }
            return Ok(objResp); ;
        }

        [AllowAnonymous]
        [HttpPost(nameof(ValidateEmail))]
        [ProducesResponseType(typeof(GenResponse<UserLoginResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ValidateEmail(EmailValidationDto user)
        {
            var objResp = await _userService.EmailRegistrationValidation(user);
            return Ok(objResp);
        }

        [AllowAnonymous]
        [HttpPost(nameof(Login))]
        [ProducesResponseType(typeof(GenResponse<UserLoginResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Login(UserLoginDto user)
        {
            var objResp = await _userService.Login(user);
            if (objResp.IsSuccess && !string.IsNullOrWhiteSpace(objResp.Result.Email))
            {
                var appUser = new AppUser()
                {
                    DisplayName = $"{objResp.Result.FirstName} {objResp.Result.LastName}",
                    Email = objResp.Result.Email,
                    Guid = objResp.Result.Guid,
                    Roles = objResp.Result.Roles.ToList<string>(),
                    Id = objResp.Result.Id,
                    CompanyId = objResp.Result.CompanyId,
                };
                objResp.Result.token = _tokenService.CreateToken(appUser, _sessionConfig.Auth.ExpireMinutes);
                objResp.Result.Roles = Array.Empty<string>();

                AppSessionManager.SetCookieSession(_contextAccessor.HttpContext, objResp?.Result?.Guid, appUser);
                _contextAccessor.HttpContext.Response.Cookies.Append(_sessionConfig.Auth.token, objResp.Result.token, new CookieOptions()
                {
                    Expires = DateTime.Now.AddMinutes(_sessionConfig.Auth.ExpireMinutes),
                    HttpOnly = _sessionConfig.Auth.HttpOnly,
                    Secure = _sessionConfig.Auth.Secure,
                    IsEssential = _sessionConfig.Auth.IsEssential,
                    SameSite = SameSiteMode.None
                });
                return Ok(objResp);
            }
            return Ok(objResp);
        }

        [AllowAnonymous]
        [HttpPost(nameof(Logout))]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public IActionResult Logout()
        {
            //HttpContext.Response.Cookies.Delete("jwt");
            _sessionContextRepo.ClearCurrentUserDataFromSession();
            return Ok(true);
        }

        [HttpGet(nameof(GetUserWithRolesById))]
        [ProducesResponseType(typeof(GenResponse<AppUser>), 200)]
        public async Task<IActionResult> GetUserWithRolesById([Required] Guid guid)
        {
            string guidValue = guid.GetType() == typeof(Guid) ? Convert.ToString(guid) : string.Empty;
            var objResp = await _userService.GetUserWithRolesByUserId(guidValue ?? string.Empty);
            return Ok(objResp);
        }


        [HttpPost(nameof(AssignRolesToUser))]
        [Auth(nameof(UserRolesEnum.Admin))]
        [ProducesResponseType(typeof(GenResponse<int>), 200)]
        public async Task<IActionResult> AssignRolesToUser(RolesToUserCreationDTO model)
        {
            var objResp = await _userRoleService.AddUserGroupRolesToUser(model);
            return StatusCode(objResp.StatCode, objResp);
        }

    }
}
