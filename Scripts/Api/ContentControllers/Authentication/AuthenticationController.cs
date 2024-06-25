using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Web;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Redbean.Extension;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthenticationController : ControllerBase
{
	private const long AccessTokenExpireSecond = 600;
	private const long RefreshTokenExpireSecond = 600;
	
	private const string userKey = "user";
	private const string tokenKey = "token";
	
	/// <summary>
	/// 사용자 로그인 및 토큰 발급
	/// </summary>
	[HttpGet]
	public async Task<IActionResult> GetUser(string userId, string version)
	{
		userId = HttpUtility.UrlDecode(userId.Decrypt());
		version = HttpUtility.UrlDecode(version.Decrypt());

		TokenResponse token;
		UserResponse user;
		
		// 사용자 유효성 검사
		try
		{
			var userRecord = await FirebaseAuth.DefaultInstance?.GetUserAsync(userId);
			user = new UserResponse
			{
				Social =
				{
					Id = userRecord.Uid.Encrypt(),
					Platform = userRecord.ProviderData[0].ProviderId
				},
				Information =
				{
					Nickname = userRecord.ProviderData[0].DisplayName
				}
			};
			
			token = GenerateTokenAsync(userId, version, userRecord.Email);
		}
		catch (FirebaseAuthException)
		{
			return BadRequest();
		}
		
		// 사용자 데이터베이스 탐색 쿼리
		var equalTo = FirebaseSetting.UserCollection?.WhereEqualTo("social.id", userId)?.Limit(1);
		var querySnapshot = await equalTo?.GetSnapshotAsync();
		if (querySnapshot.Count != 0)
		{
			user = querySnapshot.Documents[0].ToDictionary().ToConvert<UserResponse>();
			await Redis.SetUserAsync(user);
			
			return new Dictionary<string, object>
			{
				{ userKey, user },
				{ tokenKey, token }
			}.ToResponse();
		}
		
		// 새로운 사용자 데이터 저장
		await FirebaseSetting.UserCollection?.Document(userId)?.SetAsync(user.ToDocument());
		await Redis.SetUserAsync(user);
		
		return new Dictionary<string, object>
		{
			{ userKey, user.ToResponse() },
			{ tokenKey, token }
		}.ToResponse();
	}
	
	/// <summary>
	/// 에디터 전용 토큰 발급
	/// </summary>
	[HttpGet]
	public async Task<IActionResult> GetEditorAccessToken(string userId, string version)
	{
		userId = HttpUtility.UrlDecode(userId.Decrypt());
		version = HttpUtility.UrlDecode(version.Decrypt());
		
		try
		{
			var userRecord = await FirebaseAuth.DefaultInstance?.GetUserAsync(userId);

			if (App.AdministratorKey.Contains(userRecord.Email))
			{
				var tokenResponse = GenerateTokenAsync(userId, version, userRecord.Email);
				Authorization.RefreshTokens[tokenResponse.RefreshToken] = tokenResponse;
				
				return tokenResponse.ToResponse();
			}
		}
		catch (FirebaseAuthException)
		{
			return BadRequest();
		}

		return BadRequest();
	}
	
	/// <summary>
	/// 리프레시 토큰을 통한 재발급
	/// </summary>
	[HttpGet, ApiAuthorize(Role.Administrator, Role.User)]
	public IActionResult GetRefreshAccessToken(string refreshToken)
	{
		if (!Authorization.RefreshTokens.ContainsKey(refreshToken))
			return BadRequest();
		
		return RegenerateTokenAsync(Authorization.GetAuthorizationBody(Request), refreshToken).ToResponse();
	}
	
	private TokenResponse GenerateTokenAsync(string userId, string version, string email)
	{
		var accessTokenExpire = DateTime.UtcNow.AddSeconds(AccessTokenExpireSecond);
		var refreshTokenExpire = DateTime.UtcNow.AddSeconds(RefreshTokenExpireSecond);
		
		var accessToken = new JwtSecurityToken(expires: accessTokenExpire,
		                                 claims: new[]
		                                 {
			                                 new Claim(ClaimTypes.NameIdentifier, userId.Encrypt()),
			                                 new Claim(ClaimTypes.Version, version.Encrypt()),
			                                 new Claim(ClaimTypes.Role, App.AdministratorKey.Contains(email) ? Role.Administrator : Role.User)
		                                 },
		                                 signingCredentials: new SigningCredentials(new SymmetricSecurityKey(App.SecurityKey), SecurityAlgorithms.HmacSha256));
		var refreshToken = $"{Guid.NewGuid()}".Replace("-", "");
		
		Authorization.RefreshTokens[refreshToken] = new TokenResponse
		{
			AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
			RefreshToken = refreshToken,
			AccessTokenExpire = accessTokenExpire,
			RefreshTokenExpire = refreshTokenExpire,
		};
		return Authorization.RefreshTokens[refreshToken];
	}
	
	private TokenResponse RegenerateTokenAsync(AuthorizationBody body, string refreshToken)
	{
		var accessTokenExpire = DateTime.UtcNow.AddSeconds(AccessTokenExpireSecond);
		var refreshTokenExpire = DateTime.UtcNow.AddSeconds(RefreshTokenExpireSecond);
		
		var accessToken = new JwtSecurityToken(expires: accessTokenExpire,
		                                       claims: new[]
		                                       {
			                                       new Claim(ClaimTypes.NameIdentifier, body.UserId.Encrypt()),
			                                       new Claim(ClaimTypes.Version, body.Version.Encrypt()),
			                                       new Claim(ClaimTypes.Role, body.Role)
		                                       },
		                                       signingCredentials: new SigningCredentials(new SymmetricSecurityKey(App.SecurityKey), SecurityAlgorithms.HmacSha256));
		
		Authorization.RefreshTokens[refreshToken] = new TokenResponse
		{
			AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
			RefreshToken = refreshToken,
			AccessTokenExpire = accessTokenExpire,
			RefreshTokenExpire = refreshTokenExpire,
		};
		return Authorization.RefreshTokens[refreshToken];
	}
}