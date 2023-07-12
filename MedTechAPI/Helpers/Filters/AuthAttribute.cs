using Microsoft.AspNetCore.Mvc;

namespace MedTechAPI.Helpers.Filters
{
    public class AuthAttribute : TypeFilterAttribute
    {
        public AuthAttribute(params string[] roles) : base(typeof(RoleAuthorizerAttribute))
        {
            Arguments = new object[] {
            roles
        };
        }
    }
}
