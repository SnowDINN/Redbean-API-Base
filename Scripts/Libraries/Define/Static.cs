using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Redbean.Api;

public class AppSecurity
{
	public static readonly byte[] SecurityKey = Encoding.ASCII.GetBytes($"{Guid.NewGuid()}".Replace("-", ""));
}

public class AppDefaults
{
	public const string VersionScheme = "Version";
}

public class Authorization
{
	public static readonly string[] Administrators = ["mfactory86@gmail.com"];

	public static string GetUserId(HttpRequest request) => 
		GetAuthorizationBody(request).UserId;

	public static string GetVersion(HttpRequest request) =>
		request.Headers["Version"].FirstOrDefault();
	
	private static AuthorizationBody GetAuthorizationBody(HttpRequest request)
	{
		var header = request.Headers.Authorization.FirstOrDefault();
		var headerToken = header?.Replace($"{JwtBearerDefaults.AuthenticationScheme} ", "");
		var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(headerToken);

		return new AuthorizationBody
		{
			UserId = GetClaims(jwtToken, ClaimTypes.NameIdentifier).Decryption(),
			Role = GetClaims(jwtToken, ClaimTypes.Role)
		};
	}

	private static string GetClaims(JwtSecurityToken token, string type)
	{
		var value = token.Claims.FirstOrDefault(_ => _.Type == type)?.Value;
		return string.IsNullOrEmpty(value) ? string.Empty : value;
	}
}

public class GoogleAuthentication
{
	/// <summary>
	/// Google Authorization State
	/// </summary>
	public static readonly Dictionary<string, AuthenticationState> State = new();
}

public class JwtAuthentication
{
	/// <summary>
	/// JWT Refresh Tokens
	/// </summary>
	public static readonly Dictionary<string, TokenResponse> RefreshTokens = new();
}

public class Role
{
	/// <summary>
	/// [어드민 권한] Redbean.Boongsin.Administrator
	/// </summary>
	public const string Administrator = "4+LIjHPPOByQA/QRXhwOY8hmfCG3QA0XzSbKz0NNTJs=";
	
	/// <summary>
	/// [유저 권한] Redbean.Boongsin.User
	/// </summary>
	public const string User = "4+LIjHPPOByQA/QRXhwOY7s8gH8HxiwDWzk+C0icKxw=";
}

public class RedisKey
{
	public const string APP_CONFIG = nameof(APP_CONFIG);
	public const string TABLE_CONFIG = nameof(TABLE_CONFIG);
}