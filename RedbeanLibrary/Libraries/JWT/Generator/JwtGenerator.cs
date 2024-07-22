using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Redbean.JWT;

public class JwtGenerator : JwtPermission
{
	private const long AccessTokenExpireSecond = 1800;
	private const long RefreshTokenExpireSecond = 2100;
	
	public static string GenerateAdministratorTokenAsync()
	{
		var accessToken = new JwtSecurityToken(expires: DateTime.UtcNow.AddSeconds(30),
		                                       claims: new[]
		                                       {
			                                       new Claim(ClaimTypes.Role, GetPermission(PermissionType.Administrator))
		                                       },
		                                       signingCredentials: JwtSecurity.Credentials);
		
		return new JwtSecurityTokenHandler().WriteToken(accessToken);
	}
	
	public static JwtToken GenerateUserToken(string id)
	{
		var accessTokenExpire = DateTime.UtcNow.AddSeconds(AccessTokenExpireSecond);
		var refreshTokenExpire = DateTime.UtcNow.AddSeconds(RefreshTokenExpireSecond);
		
		var accessToken = new JwtSecurityToken(expires: accessTokenExpire,
		                                       claims: new[]
		                                       {
			                                       new Claim(ClaimTypes.NameIdentifier, id),
			                                       new Claim(ClaimTypes.Role, GetPermission(PermissionType.User))
		                                       },
		                                       signingCredentials: JwtSecurity.Credentials);
		var refreshToken = $"{Guid.NewGuid()}".Replace("-", "");
		
		AppToken.JwtTokens[refreshToken] = new JwtToken
		{
			AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
			RefreshToken = refreshToken,
			AccessTokenExpire = accessTokenExpire,
			RefreshTokenExpire = refreshTokenExpire,
		};
		return AppToken.JwtTokens[refreshToken];
	}
	
	public static JwtToken RegenerateUserToken(string id, string refreshToken)
	{
		var accessTokenExpire = DateTime.UtcNow.AddSeconds(AccessTokenExpireSecond);
		var refreshTokenExpire = DateTime.UtcNow.AddSeconds(RefreshTokenExpireSecond);
		
		var accessToken = new JwtSecurityToken(expires: accessTokenExpire,
		                                       claims: new[]
		                                       {
			                                       new Claim(ClaimTypes.NameIdentifier, id),
			                                       new Claim(ClaimTypes.Role, GetPermission(PermissionType.User))
		                                       },
		                                       signingCredentials: JwtSecurity.Credentials);
		
		AppToken.JwtTokens[refreshToken] = new JwtToken
		{
			AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
			RefreshToken = refreshToken,
			AccessTokenExpire = accessTokenExpire,
			RefreshTokenExpire = refreshTokenExpire,
		};
		return AppToken.JwtTokens[refreshToken];
	}
}