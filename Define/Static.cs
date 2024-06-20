using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Redbean.Api;

public class App
{
	public static readonly byte[] SecurityKey = Encoding.ASCII.GetBytes($"{Guid.NewGuid()}".Replace("-", ""));
	public static readonly string[] AdministratorKey = ["mfactory86@gmail.com"];
}

public class Authorization
{
	public static readonly Dictionary<string, TokenResponse> RefreshTokens = new();
	
	public static string GetUserId(HttpRequest request)
	{
		var header = request.Headers.Authorization.FirstOrDefault();
		var headerToken = header?.Replace($"{JwtBearerDefaults.AuthenticationScheme} ", "");
		var securityToken = new JwtSecurityTokenHandler().ReadJwtToken(headerToken);

		var identifier = securityToken.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier)?.Value;
		return string.IsNullOrEmpty(identifier) ? string.Empty : identifier.Decrypt();
	}
}

public class Role
{
	public const string Administrator = "Redbean.Boongsin.Administrator";
	public const string User = "Redbean.Boongsin.User";
}