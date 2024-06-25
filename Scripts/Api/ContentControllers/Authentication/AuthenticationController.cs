﻿using System.IdentityModel.Tokens.Jwt;
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
	public async Task<IActionResult> GetUser(string userId, string version) => 
		await GetUserAsync(userId, version);
	
	/// <summary>
	/// 에디터 전용 토큰 발급
	/// </summary>
	[HttpGet]
	public async Task<IActionResult> GetEditorAccessToken(string userId, string version) =>
		await GetEditorAccessTokenAsync(userId, version);

	/// <summary>
	/// 리프레시 토큰을 통한 재발급
	/// </summary>
	[HttpGet, ApiAuthorize(Role.User)]
	public IActionResult GetRefreshAccessToken(string refreshToken) => 
		GetRefreshAccessTokenAsync(refreshToken);
	
	private async Task<IActionResult> GetUserAsync(string userId, string version)
	{
		userId = HttpUtility.UrlDecode(userId.Decryption());
		version = HttpUtility.UrlDecode(version.Decryption());

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
					Id = userRecord.Uid.Encryption(),
					Platform = userRecord.ProviderData[0].ProviderId
				},
				Information =
				{
					Nickname = userRecord.ProviderData[0].DisplayName
				}
			};
			
			token = GenerateUserToken(userId, version);
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
	
	private async Task<IActionResult> GetEditorAccessTokenAsync(string userId, string version)
	{
		userId = HttpUtility.UrlDecode(userId.Decryption());
		version = HttpUtility.UrlDecode(version.Decryption());
		
		// 사용자 유효성 검사
		try
		{
			var user = await FirebaseAuth.DefaultInstance?.GetUserAsync(userId);
			if (App.AdministratorKey.Contains(user.Email))
				return GenerateAdministratorTokenAsync(userId, version).ToResponse();
		}
		catch (FirebaseAuthException)
		{
			return BadRequest();
		}

		return BadRequest();
	}

	private IActionResult GetRefreshAccessTokenAsync(string refreshToken)
	{
		if (!Authorization.RefreshTokens.ContainsKey(refreshToken))
			return BadRequest();
		
		return RegenerateUserToken(Authorization.GetAuthorizationBody(Request), refreshToken).ToResponse();
	}
	
	private string GenerateAdministratorTokenAsync(string userId, string version)
	{
		var accessToken = new JwtSecurityToken(expires: DateTime.UtcNow.AddSeconds(30),
		                                       claims: new[]
		                                       {
			                                       new Claim(ClaimTypes.NameIdentifier, userId.Encryption()),
			                                       new Claim(ClaimTypes.Version, version.Encryption()),
			                                       new Claim(ClaimTypes.Role, Role.Administrator)
		                                       },
		                                       signingCredentials: new SigningCredentials(new SymmetricSecurityKey(App.SecurityKey), SecurityAlgorithms.HmacSha256));
		
		return new JwtSecurityTokenHandler().WriteToken(accessToken);
	}
	
	private TokenResponse GenerateUserToken(string userId, string version)
	{
		var accessTokenExpire = DateTime.UtcNow.AddSeconds(AccessTokenExpireSecond);
		var refreshTokenExpire = DateTime.UtcNow.AddSeconds(RefreshTokenExpireSecond);
		
		var accessToken = new JwtSecurityToken(expires: accessTokenExpire,
		                                 claims: new[]
		                                 {
			                                 new Claim(ClaimTypes.NameIdentifier, userId.Encryption()),
			                                 new Claim(ClaimTypes.Version, version.Encryption()),
			                                 new Claim(ClaimTypes.Role, Role.User)
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
	
	private TokenResponse RegenerateUserToken(AuthorizationBody body, string refreshToken)
	{
		var accessTokenExpire = DateTime.UtcNow.AddSeconds(AccessTokenExpireSecond);
		var refreshTokenExpire = DateTime.UtcNow.AddSeconds(RefreshTokenExpireSecond);
		
		var accessToken = new JwtSecurityToken(expires: accessTokenExpire,
		                                       claims: new[]
		                                       {
			                                       new Claim(ClaimTypes.NameIdentifier, body.UserId.Encryption()),
			                                       new Claim(ClaimTypes.Version, body.Version.Encryption()),
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