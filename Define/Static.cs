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

	public static AuthorizationBody GetAuthorizationBody(HttpRequest request)
	{
		var header = request.Headers.Authorization.FirstOrDefault();
		var headerToken = header?.Replace($"{JwtBearerDefaults.AuthenticationScheme} ", "");
		var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(headerToken);

		return new AuthorizationBody
		{
			UserId = GetClaims(jwtToken, ClaimTypes.NameIdentifier),
			Version = GetClaims(jwtToken, ClaimTypes.Version)
		};
	}

	private static string GetClaims(JwtSecurityToken token, string type)
	{
		var value = token.Claims.FirstOrDefault(_ => _.Type == type)?.Value;
		return string.IsNullOrEmpty(value) ? string.Empty : value.Decrypt();
	}
}

public class AuthorizationBody
{
	public string UserId { get; set; } = "";
	public string Version { get; set; } = "";
}

public class Role
{
	public const string Administrator = "Redbean.Boongsin.Administrator";
	public const string User = "Redbean.Boongsin.User";
}

public class RedisKey
{
	public const string APP_CONFIG = nameof(APP_CONFIG);
}