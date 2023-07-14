using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Extensions.Services;
using OnaxTools.Dto.Identity;
using OnaxTools.Enums.Http;
using StackExchange.Redis;

namespace MedTechAPI.Extensions.Middleware
{
    public class AppSessionManager
    {
        public readonly RequestDelegate _next;

        public AppSessionManager(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, TokenService tokenService)
        {
            context.Request.Cookies.TryGetValue(AppConstants.CookieUserId, out string cookieValue);
            var objUser = await tokenService.ValidateToken(context, refreshBeforeExpiry: true);
            try
            {
                string sessionData;
                if (cookieValue == null && !objUser.IsSuccess)
                {
                    SetCookieSession(context, objUser?.Result?.Guid);
                }
                else
                {
                    sessionData = context.Session.GetString(objUser?.Result?.Guid ?? cookieValue);
                    if (sessionData != null)
                    {
                        if(!String.IsNullOrWhiteSpace(objUser?.Result?.Guid) && (cookieValue != objUser?.Result?.Guid))
                        {
                            context.Session.Remove(cookieValue);
                        }
                        AppSessionData<AppUser> appSessionData = System.Text.Json.JsonSerializer.Deserialize<AppSessionData<AppUser>>(sessionData);
                        if (appSessionData == null)
                        {
                            SetCookieSession(context, objUser?.Result?.Guid);
                        }
                        if (appSessionData != null)
                        {
                            appSessionData.LastUpdated = DateTime.UtcNow;
                            if (objUser.IsSuccess && objUser.StatCode == (int)StatusCodeEnum.OK)
                            {
                                appSessionData.Data = objUser.Result;
                                appSessionData.Email = objUser.Result.Email;
                            }
                            context.Session.SetString(appSessionData.SessionId, System.Text.Json.JsonSerializer.Serialize(appSessionData));
                        }
                    }
                    else
                    {
                        SetCookieSession(context, objUser?.Result?.Guid);
                    }
                }
            }
            catch (Exception ex)
            {
                OnaxTools.Logger.LogException(ex);
            }
            finally
            {
                await _next(context);
            }

        }

        #region Class helpers
        public static void SetCookieSession(HttpContext context, string sessionId = null, AppUser appUser = null)
        {
            sessionId = string.IsNullOrWhiteSpace(sessionId) ? Guid.NewGuid().ToString() : sessionId;
            //TODO: Convert from string Type to AppUser Type
            var userSession = new AppSessionData<AppUser>
            {
                SessionId = sessionId,
                Data = appUser,
            };
            context.Session.SetString(sessionId, System.Text.Json.JsonSerializer.Serialize(userSession));
            context.Response.Cookies.Append(AppConstants.CookieUserId, sessionId);
        }
        #endregion
    }

}
