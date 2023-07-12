using Common.DbAccess;
using Common.Interface;
using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.Entities;
using MedTechAPI.Domain.Entities.ProfileManagement;
using MedTechAPI.Persistence;
using Microsoft.EntityFrameworkCore;
using OnaxTools.Dto.Http;
using OnaxTools.Enums.Http;

namespace MedTechAPI.AppCore.Repository
{
    public class UserGroupRepository : IUserGroupRepository
    {
        private readonly ILogger<UserGroupRepository> _logger;
        private readonly ISqlDataAccess _sqlDataAccess;
        private readonly AppDbContext _context;
        private readonly ICacheService _cacheService;

        public UserGroupRepository(
            ILogger<UserGroupRepository> logger, ISqlDataAccess sqlDataAccess, AppDbContext _context, ICacheService cacheService)
        {
            this._context = _context;
            this._cacheService = cacheService;
            this._sqlDataAccess = sqlDataAccess;
            _logger = logger;
        }
        public async Task<GenResponse<IEnumerable<UserGroup>>> GetAllUserGroups()
        {
            GenResponse<IEnumerable<UserGroup>> allRoles = new() {Result =  Array.Empty<UserGroup>()};
            try
            {
                var cachedRoles = await _cacheService.GetData<IEnumerable<UserGroup>>("allUserGroups");
                if (cachedRoles == null || !cachedRoles.Any())
                {
                    allRoles.Result = await _sqlDataAccess.GetData<UserGroup, dynamic>("select * from \"UserGroup\" ur  where ur.\"IsActive\"  = @isActive  and ur.\"IsDeleted\" = @isDeleted ", new { isActive = true, isDeleted = false });
                    if (allRoles != null && allRoles.Result.Any())
                    {
                        allRoles.IsSuccess = true;
                        _ = _cacheService.SetData<IEnumerable<UserGroup>>("allUserGroups", allRoles.Result, 60 * 60);
                    }
                }
                else
                {
                    allRoles.Result = cachedRoles;
                    allRoles.IsSuccess = true;
                }
                _logger.LogInformation(System.Text.Json.JsonSerializer.Serialize(allRoles));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return GenResponse<IEnumerable<UserGroup>>.Failed("An error occured. Kindly retry or contact support.");
            }
            return allRoles;
        }

        public async Task<GenResponse<UserGroup>> InsertRole(UserRoleCreationDTO role)
        {
            if (role == null || string.IsNullOrWhiteSpace(role.RoleName))
            {
                return GenResponse<UserGroup>.Failed("Invalid user role parameters passed.");
            }
            role.RoleName = string.Concat(role.RoleName.Take(1).FirstOrDefault().ToString().ToUpper(), String.Join("", role.RoleName.Take(new Range(start: 1, end: role.RoleName.Length))).ToLower());
            GenResponse<UserGroup> userRole = new()
            {
                Result = new UserGroup
                {
                    GroupName = role.RoleName,
                    GroupDescription = role.RoleDescription
                }
            };
            try
            {
                _context.UserGroup.Add(userRole.Result);
                var objSaved = await _context.SaveChangesAsync();
                if (objSaved > 0)
                {
                    userRole.IsSuccess = true;
                    userRole.Message = $"User role '{userRole.Result.GroupName}' successfully created.";
                }
                else
                {
                    userRole.Message = $"Unable to add role '{userRole.Result.GroupName}'. Kindly try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.GetType() == typeof(Npgsql.PostgresException))
                    {
                        Npgsql.PostgresException except = ex.InnerException as Npgsql.PostgresException;
                        if (except.SqlState == "23505")
                        {
                            userRole = GenResponse<UserGroup>.Failed($"Unable to add duplicate role '{role.RoleName}'. This role already exists.");
                            userRole.IsSuccess = false;
                        }
                    }
                }
            }
            return userRole;
        }

        public async Task<GenResponse<List<UserGroup>>> GetUserGroupsByIds(int[] roleId)
        {
            GenResponse<List<UserGroup>> role = new();
            try
            {
                //var multipleSingleRole = await _sqlDataAccess.GetData<UserGroup, dynamic>("select * from \"UserRole\" ur  where ur.\"IsActive\"  = @isActive  and ur.\"IsDeleted\" = @isDeleted and ur.\"Id\" = @Id", new { isActive = true, isDeleted = false, Id = roleId });
                var multipleSingleRole = await GetAllUserGroups();
                if (multipleSingleRole != null && multipleSingleRole.Result.Any())
                {
                    role.Result = multipleSingleRole.Result.Where(m=> roleId.Contains(m.Id)).ToList();
                    if (role.Result != null)
                    { role.IsSuccess = true; }
                    else
                    {
                        role.StatCode = (int)StatusCodeEnum.NotFound;
                        role.IsSuccess = false;
                        role.Message = role.Error = "No record found for this usergroup.";
                    }
                }
                else
                {
                    role.Message = role.Error = $"No user role with id {roleId} was found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            return role;
        }

        public async Task<GenResponse<int>> AddUserGroupRolesToUser(RolesToUserCreationDTO model, CancellationToken ct = default)
        {
            int countUpdated = 0;
            try
            {
                if (model == null || model.UserRoleIds == null || !model.UserRoleIds.Any())
                {
                    return GenResponse<int>.Failed("Invalid RoleIds passed", StatusCodeEnum.BadRequest);
                }
                List<UserProfileGroup> userProfileRoles = new();
                model.UserRoleIds.ForEach(m => userProfileRoles.Add(new UserProfileGroup { UserGroupId = m, UserProfileId = model.UserProfileId, CompanyId = model.CompanyId }));
                _context.UserProfileGroups.AddRange(userProfileRoles);
                countUpdated = await _context.SaveChangesAsync(ct);

                if (countUpdated > 0)
                {
                    _ = RemoveCachedUserWithRolesByUserId((await _context.UserProfiles.FirstOrDefaultAsync(m => m.Id == model.UserProfileId)).Guid.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.GetType() == typeof(Npgsql.PostgresException))
                    {
                        Npgsql.PostgresException except = ex.InnerException as Npgsql.PostgresException;
                        if (except.SqlState == "23505")
                        {
                            return GenResponse<int>.Failed($"Unable to add duplicate role(s). One or more roles already exists for this user.", StatusCodeEnum.NotImplemented);
                        }
                    }
                }
                return GenResponse<int>.Failed("An error occured. Kindly try again.", StatusCodeEnum.ServerError);
            }
            return countUpdated > 0 ? GenResponse<int>.Success(countUpdated) : GenResponse<int>.Failed($"Failed to assign role(s) to user.");
        }


        #region Helpers
        private async Task<bool> RemoveCachedUserWithRolesByUserId(string UserGuid)
        {
            return await _cacheService.RemoveData($"UserWithRoles_{UserGuid}");
        }
        #endregion

    }
}
