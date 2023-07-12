using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.Domain.Entities.ProfileManagement;
using MedTechAPI.Domain.Enums;
using OnaxTools.Dto.Http;

namespace AppTests.Features
{
    public class UserGroupTest
    {
        private readonly Mock<IUserGroupRepository> _userGroupService;
        private readonly IUserGroupRepository _handler;
        //private readonly CancellationToken _ct = default!;
        public UserGroupTest()
        {
            _userGroupService = new Mock<IUserGroupRepository>();
            _handler = _userGroupService.Object;
            //arrange
            _userGroupService.Setup((svc) => svc.GetAllUserGroups()).ReturnsAsync(() =>
            {
                return new GenResponse<IEnumerable<UserGroup>>
                {
                    IsSuccess = true,
                    Result = new List<UserGroup>()
                    {
                        new UserGroup { GroupName = UserRolesEnum.Admin.ToString(), GroupDescription = "This is the Adminstrative role.", GroupRight = nameof(UserRolesEnum.Admin), AllowApprove = true, AllowdDelete = true, AllowNew = true, AllowEdit = true, CompanyId = 1},
                        new UserGroup { GroupName = UserRolesEnum.Viewer.ToString(), GroupDescription = "", GroupRight =nameof(UserRolesEnum.Viewer), CompanyId = 2},
                        new UserGroup { GroupName = UserRolesEnum.Approver.ToString(), GroupDescription = "", GroupRight =nameof(UserRolesEnum.Approver), AllowApprove = true, CompanyId = 3},
                        new UserGroup { GroupName = UserRolesEnum.Member.ToString(), GroupDescription = "", GroupRight =nameof(UserRolesEnum.Member), CompanyId = 4},
                        new UserGroup {GroupName = UserRolesEnum.Contributor.ToString(), GroupDescription = "This is the default role for all users", GroupRight =nameof(UserRolesEnum.Contributor), AllowNew = true, CompanyId = 5},
                    }
                };
            });


        }


        [Fact]
        public async Task CheckHasAtLeastOneRole()
        {            
            //Act
            var hasRoles = await _handler.GetAllUserGroups();

            //Assert

#pragma warning disable CS8604 // Possible null reference argument.
            Assert.NotEmpty(hasRoles?.Result);
#pragma warning restore CS8604 // Possible null reference argument.
        }


        [Fact]
        public async Task IsOfTypeGenRespomse()
        {
            //act
            var result = await _handler.GetAllUserGroups();
            //assert
            Assert.IsType<GenResponse<IEnumerable<UserGroup>>>(result);

        }



    }
}
