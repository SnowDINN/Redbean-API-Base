using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace Redbean.JWT;

public static class JwtExtension
{
	public static string GetUserId(this ControllerBase controller) => 
		GetAuthorizationBody(controller).UserId;

	public static string GetVersion(this ControllerBase controller) =>
		controller.Request.Headers["Version"].FirstOrDefault();
	
	private static JwtBody GetAuthorizationBody(ControllerBase controller)
	{
		var header = controller.Request.Headers.Authorization.FirstOrDefault();
		var headerToken = header?.Replace($"{JwtBearerDefaults.AuthenticationScheme} ", "");
		var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(headerToken);

		return new JwtBody
		{
			UserId = GetClaims(jwtToken, ClaimTypes.NameIdentifier),
			Role = GetClaims(jwtToken, ClaimTypes.Role)
		};
	}

	private static string GetClaims(JwtSecurityToken token, string type)
	{
		var value = token.Claims.FirstOrDefault(_ => _.Type == type)?.Value;
		return string.IsNullOrEmpty(value) ? string.Empty : value;
	}
}