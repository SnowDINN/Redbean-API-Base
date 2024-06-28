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
	private const long AccessTokenExpireSecond = 1800;
	private const long RefreshTokenExpireSecond = 2100;
	
	private const string userKey = "user";
	private const string tokenKey = "token";

	/// <summary>
	/// 사용자 로그인 및 토큰 발급
	/// </summary>
	[HttpGet]
	public async Task<IActionResult> GetUser(string id) => 
		await GetUserAsync(id);
	
	/// <summary>
	/// 에디터 전용 토큰 발급
	/// </summary>
	[HttpGet]
	public async Task<IActionResult> GetEditorAccessToken(string email) =>
		await GetEditorAccessTokenAsync(email);

	/// <summary>
	/// 리프레시 토큰을 통한 재발급
	/// </summary>
	[HttpGet, ApiAuthorize(Role.User)]
	public async Task<IActionResult> GetRefreshAccessToken(string refreshToken) => 
		await GetRefreshAccessTokenAsync(refreshToken);
	
	private async Task<IActionResult> GetUserAsync(string id)
	{
		TokenResponse token;
		UserResponse user;
		
		// 사용자 유효성 검사
		try
		{
			id = HttpUtility.UrlDecode(id.Decryption());
			
			var userRecord = await FirebaseAuth.DefaultInstance?.GetUserAsync(id);
			user = new UserResponse
			{
				Social =
				{
					Id = userRecord.Uid.Encryption(),
					Platform = userRecord.ProviderData[0].ProviderId
				},
				Information =
				{
					Nickname = userRecord.ProviderData[0].DisplayName
				}
			};
			
			token = GenerateUserToken(id);
		}
		catch
		{
			return BadRequest();
		}
		
		// 사용자 데이터베이스 탐색 쿼리
		var equalTo = FirebaseSetting.UserCollection?.WhereEqualTo("social.id", id)?.Limit(1);
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
		await FirebaseSetting.UserCollection?.Document(id)?.SetAsync(user.ToDocument());
		await Redis.SetUserAsync(user);
		
		return new Dictionary<string, object>
		{
			{ userKey, user.ToResponse() },
			{ tokenKey, token }
		}.ToResponse();
	}
	
	private Task<IActionResult> GetEditorAccessTokenAsync(string email)
	{
		var completionSource = new TaskCompletionSource<IActionResult>();
		
		// 사용자 유효성 검사
		try
		{
			email = HttpUtility.UrlDecode(email.Decryption());
			
			if (Authorization.Administrators.Contains(email))
				completionSource.SetResult(GenerateAdministratorTokenAsync().ToResponse());
			else
				completionSource.SetResult(BadRequest());
		}
		catch
		{
			completionSource.SetResult(BadRequest());
		}

		return completionSource.Task;
	}

	private Task<IActionResult> GetRefreshAccessTokenAsync(string refreshToken)
	{
		var completionSource = new TaskCompletionSource<IActionResult>();
		
		if (!JwtAuthentication.RefreshTokens.ContainsKey(refreshToken))
			completionSource.SetResult(BadRequest());
		
		completionSource.SetResult(RegenerateUserToken(Authorization.GetUserId(Request), refreshToken).ToResponse());
		return completionSource.Task;
	}
	
	private string GenerateAdministratorTokenAsync()
	{
		var accessToken = new JwtSecurityToken(expires: DateTime.UtcNow.AddSeconds(30),
		                                       claims: new[]
		                                       {
			                                       new Claim(ClaimTypes.Role, Role.Administrator)
		                                       },
		                                       signingCredentials: new SigningCredentials(new SymmetricSecurityKey(AppSecurity.SecurityKey), SecurityAlgorithms.HmacSha256));
		
		return new JwtSecurityTokenHandler().WriteToken(accessToken);
	}
	
	private TokenResponse GenerateUserToken(string id)
	{
		var accessTokenExpire = DateTime.UtcNow.AddSeconds(AccessTokenExpireSecond);
		var refreshTokenExpire = DateTime.UtcNow.AddSeconds(RefreshTokenExpireSecond);
		
		var accessToken = new JwtSecurityToken(expires: accessTokenExpire,
		                                 claims: new[]
		                                 {
			                                 new Claim(ClaimTypes.NameIdentifier, id.Encryption()),
			                                 new Claim(ClaimTypes.Role, Role.User)
		                                 },
		                                 signingCredentials: new SigningCredentials(new SymmetricSecurityKey(AppSecurity.SecurityKey), SecurityAlgorithms.HmacSha256));
		var refreshToken = $"{Guid.NewGuid()}".Replace("-", "");
		
		JwtAuthentication.RefreshTokens[refreshToken] = new TokenResponse
		{
			AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
			RefreshToken = refreshToken,
			AccessTokenExpire = accessTokenExpire,
			RefreshTokenExpire = refreshTokenExpire,
		};
		return JwtAuthentication.RefreshTokens[refreshToken];
	}
	
	private TokenResponse RegenerateUserToken(string id, string refreshToken)
	{
		var accessTokenExpire = DateTime.UtcNow.AddSeconds(AccessTokenExpireSecond);
		var refreshTokenExpire = DateTime.UtcNow.AddSeconds(RefreshTokenExpireSecond);
		
		var accessToken = new JwtSecurityToken(expires: accessTokenExpire,
		                                       claims: new[]
		                                       {
			                                       new Claim(ClaimTypes.NameIdentifier, id.Encryption()),
			                                       new Claim(ClaimTypes.Role, Role.User)
		                                       },
		                                       signingCredentials: new SigningCredentials(new SymmetricSecurityKey(AppSecurity.SecurityKey), SecurityAlgorithms.HmacSha256));
		
		JwtAuthentication.RefreshTokens[refreshToken] = new TokenResponse
		{
			AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
			RefreshToken = refreshToken,
			AccessTokenExpire = accessTokenExpire,
			RefreshTokenExpire = refreshTokenExpire,
		};
		return JwtAuthentication.RefreshTokens[refreshToken];
	}
}