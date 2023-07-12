using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Extensions.Services;
using Microsoft.Extensions.Options;
using OnaxTools.Dto.Http;
using OnaxTools.Dto.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace MedTechAPI.AppCore.Repository
{
    public class AppSessionContextRepository : IAppSessionContextRepository
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly SessionConfig _sessionConfig;

        //private readonly TokenService _tokenService;

        public AppSessionContextRepository(IHttpContextAccessor contextAccessor
            , IOptions<SessionConfig> sessionConfig)//, TokenService tokenService)
        {
            _contextAccessor = contextAccessor;
            _sessionConfig = sessionConfig.Value;
            //_tokenService = tokenService;
        }

        public AppSessionData<AppUser> GetUserDataFromSession()
        {
            AppSessionData<AppUser> objResp = new();
            try
            {
                string cookieValue = null;
                if (_contextAccessor != null && _contextAccessor.HttpContext != null)
                {
                    _contextAccessor.HttpContext.Request.Cookies.TryGetValue(AppConstants.CookieUserId, out cookieValue);
                }
                if (String.IsNullOrWhiteSpace(cookieValue))
                {
                    return objResp;
                }
                else
                {
                    byte[] userDataFromSession = null;
                    if (_contextAccessor != null && _contextAccessor.HttpContext != null)
                    {
                        _contextAccessor.HttpContext.Session.TryGetValue(key: cookieValue, out userDataFromSession);
                    }
                    if (userDataFromSession != null && userDataFromSession.Any())
                    {
                        objResp = System.Text.Json.JsonSerializer.Deserialize<AppSessionData<AppUser>>(Encoding.UTF8.GetString(userDataFromSession));
                    }
                }
            }
            catch (Exception ex)
            {
                OnaxTools.Logger.LogException(ex);
            }
            return objResp;
        }

        public void ClearCurrentUserDataFromSession()
        {
            try
            {
                string cookieValue = null;
                if (_contextAccessor != null && _contextAccessor.HttpContext != null)
                {
                    var authHeader = _contextAccessor.HttpContext.Request.Headers.Authorization.FirstOrDefault(m => m != null && m.StartsWith("Bearer"));
                    _contextAccessor.HttpContext.Request.Cookies.TryGetValue(_sessionConfig.Auth.token, out cookieValue);
                    if (authHeader != null || !string.IsNullOrWhiteSpace(cookieValue))
                    {
                        string authToken = !string.IsNullOrWhiteSpace(cookieValue) ? cookieValue : authHeader.Split(" ")[1];
                        var jwtTokenHandler = new JwtSecurityTokenHandler();

                        JwtSecurityToken read = jwtTokenHandler.ReadJwtToken(authToken);
                        var userId = read.Claims.FirstOrDefault(m=> m.Type.Equals("guid",StringComparison.InvariantCultureIgnoreCase))?.Value;
                        if (!string.IsNullOrWhiteSpace(userId))
                        {
                            _contextAccessor.HttpContext.Session.Remove(userId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnaxTools.Logger.LogException(ex);
            }
        }
    }
}
