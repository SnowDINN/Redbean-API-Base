using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Redbean.Api;

public class ApiAuthorizeAttribute : AuthorizeAttribute
{
	public ApiAuthorizeAttribute(params string[] roles)
	{
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
		Roles = string.Join(",", roles);
	}
}