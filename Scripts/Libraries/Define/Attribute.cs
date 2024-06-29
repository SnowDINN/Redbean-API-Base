using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Redbean.Api;

public class HttpAuthorizeAttribute : AuthorizeAttribute
{
	public HttpAuthorizeAttribute(params string[] roles)
	{
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
		Roles = string.Join(",", roles);
	}
}

public class HttpSchema : ProducesResponseTypeAttribute
{
	public HttpSchema(Type type) : base(type, 200)
	{
		Type = type;
		StatusCode = 200;
	}
}