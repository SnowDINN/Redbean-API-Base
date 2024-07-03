﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Redbean.Extension;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class GetAuthorizeController : ControllerBase
{
	private const long AccessTokenExpireSecond = 1800;
	private const long RefreshTokenExpireSecond = 2100;

	/// <summary>
	/// 사용자 로그인 및 토큰 발급
	/// </summary>
	[HttpGet, HttpSchema(typeof(UserAndTokenResponse))]
	public async Task<IActionResult> GetAccessTokenAndUser(string id) => 
		await GetAccessTokenAndUserAsync(id);
	
	/// <summary>
	/// 리프레시 토큰을 통한 재발급
	/// </summary>
	[HttpGet, HttpSchema(typeof(TokenResponse)), HttpAuthorize(Role.User)]
	public async Task<IActionResult> GetAccessTokenRefresh(string token) => 
		await GetRefreshAccessTokenAsync(token);
	
	private async Task<IActionResult> GetAccessTokenAndUserAsync(string id)
	{
		TokenResponse token;
		UserResponse user;
		
		// 사용자 유효성 검사
		try
		{
			id = id.Decryption();
			
			var userRecord = await FirebaseSetting.Authentication?.GetUserAsync(id);
			user = new UserResponse
			{
				Information =
				{
					Id = userRecord.Uid.Encryption(),
					Nickname = userRecord.DisplayName
				},
				Log =
				{
					LastConnected = $"{DateTime.UtcNow}"
				}
			};
			if (userRecord.ProviderData.Length > 0)
			{
				user.Social.Profile = userRecord.ProviderData[0].PhotoUrl;
				user.Social.Platform = userRecord.ProviderData[0].ProviderId;
			}
			
			token = GenerateUserToken(id);
		}
		catch
		{
			return this.ToPublishCode(1);
		}
		
		// 기존 사용자 탐색
		var userDocument = FirebaseSetting.UserCollection?.Document(id);
		var userSnapshot = await userDocument?.GetSnapshotAsync();
		if (userSnapshot.Exists)
		{
			user = userSnapshot.ToDictionary().ToConvert<UserResponse>();
			user.Log.LastConnected = $"{DateTime.UtcNow}";
			
			// 마지막 로그인 기록 갱신
			await FirebaseSetting.UserCollection?.Document(id)?.SetAsync(user.ToDocument());
			await Redis.SetUserAsync(id, user);

			return new UserAndTokenResponse
			{
				User = user,
				Token = token
			}.ToPublish();
		}
		
		// 새로운 사용자 데이터 저장
		await FirebaseSetting.UserCollection?.Document(id)?.SetAsync(user.ToDocument());
		await Redis.SetUserAsync(id, user);
		
		return new UserAndTokenResponse
		{
			User = user,
			Token = token
		}.ToPublish();
	}

	private Task<IActionResult> GetRefreshAccessTokenAsync(string refreshToken)
	{
		var completionSource = new TaskCompletionSource<IActionResult>();
		completionSource.SetResult(JwtAuthentication.Tokens.ContainsKey(refreshToken)
			                           ? RegenerateUserToken(Authorization.GetUserId(Request), refreshToken).ToPublish()
			                           : null);

		return completionSource.Task;
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
		
		JwtAuthentication.Tokens[refreshToken] = new TokenResponse
		{
			AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
			RefreshToken = refreshToken,
			AccessTokenExpire = accessTokenExpire,
			RefreshTokenExpire = refreshTokenExpire,
		};
		return JwtAuthentication.Tokens[refreshToken];
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
		
		JwtAuthentication.Tokens[refreshToken] = new TokenResponse
		{
			AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
			RefreshToken = refreshToken,
			AccessTokenExpire = accessTokenExpire,
			RefreshTokenExpire = refreshTokenExpire,
		};
		return JwtAuthentication.Tokens[refreshToken];
	}
}