using Common.Interface;
using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.AppCore.Navigation.Interfaces;
using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.DTO.Navigation;
using MedTechAPI.Domain.Entities.ProfileManagement;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using MedTechAPI.Persistence;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OnaxTools.Dto.Http;
using System.Collections.Generic;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MedTechAPI.Extensions.Services
{
    public class TokenService
    {
        private readonly string _encryptionKey;
        //private readonly AppDbContext _context;
        //private readonly ICacheService _cacheService;
        private readonly IUserGroupRepository _userGroupRepo;
        private readonly IMenuRepository _menuRepo;
        private readonly SessionConfig _sessionConfig;

        private const string _applicationMenu_ = $"{nameof(_applicationMenu_)}_Global";

        public TokenService(string EncryptionKey, IOptions<SessionConfig> sessionConfig
            //, AppDbContext context
            //, ICacheService cacheService
            , IUserGroupRepository userGroupRepo
            , IMenuRepository menuRepo)
        {
            _encryptionKey = EncryptionKey;
            //_context = context;
            //_cacheService = cacheService;
            _userGroupRepo = userGroupRepo;
            _menuRepo = menuRepo;
            _sessionConfig = sessionConfig.Value;
        }

        public string CreateToken(AppUser user, int expireInMins = 15)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("guid", user.Guid),
                new Claim("Id", $"{user.Id}"),
                new Claim("CompanyId", $"{user.CompanyId}")
            };
            if (user.Roles != null && user.Roles.Count >= 0)
            {
                claims.Add(new Claim(ClaimTypes.Role, System.Text.Json.JsonSerializer.Serialize(user.Roles), JsonClaimValueTypes.JsonArray));
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_encryptionKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expireInMins),
                SigningCredentials = creds,
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<GenResponse<AppUser>> ValidateToken(HttpContext context, bool refreshBeforeExpiry = false)
        {
            var objResp = new GenResponse<AppUser>();
            if (context == null)
            {
                return GenResponse<AppUser>.Failed("Invalid/Expired token credentials");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_encryptionKey);
            try
            {
                var authHeader = context.Request.Headers.Authorization.FirstOrDefault(m => m != null && m.StartsWith("Bearer"));
                context.Request.Cookies.TryGetValue(_sessionConfig.Auth.token, out string cookieValue);
                if (authHeader != null || !string.IsNullOrWhiteSpace(cookieValue))
                {
                    string authToken = !string.IsNullOrWhiteSpace(cookieValue) ? cookieValue : authHeader.Split(" ")[1];
                    //TODO: Delete below
                    var eventual = GetUserClaims(authToken);
                    var jwtTokenHandler = new JwtSecurityTokenHandler();

                    jwtTokenHandler.ValidateToken(authToken, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    JwtSecurityToken jwtoken = (JwtSecurityToken)validatedToken;

                    if (jwtoken != null)
                    {
                        Dictionary<string, string> tokenValues = new Dictionary<string, string>();
                        foreach (var claim in jwtoken.Claims)
                        {
                            if (tokenValues.ContainsKey(claim.Type))
                            {
                                tokenValues[claim.Type] = tokenValues[claim.Type] + $",{claim.Value}";
                            }
                            else
                            {
                                tokenValues.Add(claim.Type, claim.Value);
                            }
                        }
                        if (tokenValues.Any())
                        {
                            var appUser = new AppUser
                            {
                                Email = tokenValues["email"],
                                DisplayName = tokenValues["unique_name"],
                                Guid = tokenValues["guid"],
                                Id = Convert.ToInt64(tokenValues["Id"]),
                                CompanyId = Convert.ToInt32(tokenValues["CompanyId"]),
                            };
                            tokenValues.TryGetValue("role", out string userRoles);
                            appUser.Roles = !string.IsNullOrWhiteSpace(userRoles) ? userRoles.Split(',').ToList<string>() : new List<string>();
                            objResp = GenResponse<AppUser>.Success(appUser);
                            #region Sliding Expiration 
                            if (refreshBeforeExpiry)
                            {

                                TimeSpan timeElapsed = DateTime.UtcNow.Subtract(jwtoken.ValidFrom);
                                TimeSpan timeRemaining = jwtoken.ValidTo.Subtract(DateTime.UtcNow);
                                //if more than half of the timeout interval has elapsed.
                                if (timeRemaining < timeElapsed)
                                {
                                    var prolongedToken = this.CreateToken(objResp.Result, _sessionConfig.Auth.ExpireMinutes);
                                    context.Response.Headers.Remove("Set-Authorization");
                                    context.Response.Headers.Add("Set-Authorization", prolongedToken);

                                    context.Response.Cookies.Append(_sessionConfig.Auth.token, prolongedToken, new CookieOptions()
                                    {
                                        Expires = DateTime.Now.AddMinutes(_sessionConfig.Auth.ExpireMinutes),
                                        HttpOnly = _sessionConfig.Auth.HttpOnly,
                                        Secure = _sessionConfig.Auth.Secure,
                                        IsEssential = _sessionConfig.Auth.IsEssential,
                                        SameSite = SameSiteMode.None
                                    });
                                }
                            }
                            #endregion

                            #region VALIDATE GroupRight to access route
                            //1. Get all topLevel Menus
                            GenResponse<IEnumerable<AllNavigationMenuFlat>> allTopLevelMenu = await _menuRepo.GetAllNavigationMenuFlat();

                            //2. Get all UserGroups/Roles
                            GenResponse<IEnumerable<UserGroup>> allUserGroups = await _userGroupRepo.GetAllUserGroups();

                            List<string> allowedUserRoutes = new();
                            if (allUserGroups != null && allUserGroups.IsSuccess)
                            {
                                //3. Select the actionRoutes the user can access.
                                allowedUserRoutes = allUserGroups.Result.Where(m => appUser.Roles.Contains(m.GroupName) && m.CompanyId == appUser.CompanyId).Select(m => m.GroupRight).ToList();
                            }
                            if (allTopLevelMenu != null && allTopLevelMenu.IsSuccess && allowedUserRoutes.Count > 0)
                            {
                                //4. Filter the menu to only "allowedUserRoutes"
                                List<AllNavigationMenuFlat> objModulesFlat = new List<AllNavigationMenuFlat>();
                                //List<MenuModules> objModules = new();// { MenuActions = new List<MenuActions>() };
                                foreach (var item in allowedUserRoutes)
                                {
                                    if (string.IsNullOrWhiteSpace(item))
                                        continue;
                                    string[] items = item.Split(',');
                                    foreach (string actionName in items)
                                    {
                                        AllNavigationMenuFlat actionSelection = allTopLevelMenu.Result.FirstOrDefault(m => m.CompanyId == appUser.CompanyId && m.ActionName.Equals(actionName, StringComparison.OrdinalIgnoreCase));
                                        if (actionSelection != null)
                                        {
                                            objModulesFlat.Add(actionSelection);
                                        }
                                    }
                                }
                                if (objModulesFlat != null && objModulesFlat.Count > 0)
                                {
                                    //var urlPath = context.Request.Path;
                                    var urlPath = context.Request.RouteValues;
                                    string fullUrlPath = $"/{urlPath["controller"]}/{urlPath["action"]}";
                                    if (!fullUrlPath.Equals("/Auth/Logout", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        if (!objModulesFlat.Any(m => m.UrlPath.Equals(fullUrlPath, StringComparison.OrdinalIgnoreCase)))
                                        {
                                            return GenResponse<AppUser>.Failed("Unauthorized to access this resource", OnaxTools.Enums.Http.StatusCodeEnum.Unauthorized);
                                        }
                                    }
                                    #region Possibly not needed here.. But needed for multiple UserGroup Rights
                                    var item = objModulesFlat.GroupBy(m => m.ControllerName).Select(m => new MenuActions
                                    {
                                        Id = m.First().Id,
                                        OrderPriority = m.First().OrderPriority,
                                        CompanyId = m.First().CompanyId,
                                        ActionName = m.First().ActionName,
                                        UrlPath = m.First().UrlPath,
                                        DisplayName = m.First().DisplayName,
                                        IconClass = m.First().IconClass,
                                        ActionDescription = m.First().ActionDescription,
                                        AllowAnonymous = m.First().AllowAnonymous,
                                        MenuControllerId = m.First().MenuControllerId
                                    }).ToList();
                                    #endregion
                                }
                            }
                            else
                            {
                            }
                            #endregion

                        }
                        else
                        {
                            objResp = GenResponse<AppUser>.Failed("Invalid token credentials");
                        }
                    }
                }
                else { objResp = GenResponse<AppUser>.Failed("Invalid token credentials"); }
            }
            catch (SecurityTokenExpiredException err)
            {
                OnaxTools.Logger.LogException(err, "[CommonHelpers][ValidateJwt]");
                objResp = GenResponse<AppUser>.Failed("Expired token credentials");
            }
            catch (Exception ex)
            {
                OnaxTools.Logger.LogException(ex, "[CommonHelpers][ValidateJwt]");
                objResp = GenResponse<AppUser>.Failed("Invalid token credentials");
            }
            return objResp;
        }


        public GenResponse<AppUser> GetUserClaims(string authToken)
        {
            GenResponse<AppUser> objResp = new() { Result = new() };
            var key = Encoding.ASCII.GetBytes(_encryptionKey);
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            jwtTokenHandler.ValidateToken(authToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            JwtSecurityToken jwtoken = (JwtSecurityToken)validatedToken;

            if (jwtoken != null)
            {
                Dictionary<string, string> tokenValues = new Dictionary<string, string>();
                foreach (var claim in jwtoken.Claims)
                {
                    if (tokenValues.ContainsKey(claim.Type))
                    {
                        tokenValues[claim.Type] = tokenValues[claim.Type] + $",{claim.Value}";
                    }
                    else
                    {
                        tokenValues.Add(claim.Type, claim.Value);
                    }
                }
                if (tokenValues.Any())
                {
                    var appUser = new AppUser
                    {
                        Email = tokenValues["email"],
                        DisplayName = tokenValues["unique_name"],
                        Guid = tokenValues["guid"],
                        Id = Convert.ToInt64(tokenValues["Id"]),
                        CompanyId = Convert.ToInt32(tokenValues["CompanyId"]),
                    };
                    tokenValues.TryGetValue("role", out string userRoles);
                    appUser.Roles = !string.IsNullOrWhiteSpace(userRoles) ? userRoles.Split(',').ToList<string>() : new List<string>();
                    return GenResponse<AppUser>.Success(appUser);
                }
                else
                {
                    objResp = GenResponse<AppUser>.Failed("Invalid token credentials");
                }
            }
            return objResp;
        }

    }

}
