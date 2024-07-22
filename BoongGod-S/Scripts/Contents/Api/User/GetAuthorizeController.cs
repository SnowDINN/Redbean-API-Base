using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;
using Redbean.Firebase;
using Redbean.JWT;
using Redbean.Redis;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class GetAuthorizeController : ControllerBase
{
	/// <summary>
	/// 사용자 로그인 및 토큰 발급
	/// </summary>
	[HttpGet, HttpSchema(typeof(UserAndTokenResponse))]
	public async Task<IActionResult> GetAccessTokenAndUser(string id) => 
		await GetAccessTokenAndUserAsync(id);
	
	/// <summary>
	/// 리프레시 토큰을 통한 재발급
	/// </summary>
	[HttpGet, HttpSchema(typeof(TokenResponse)), HttpAuthorize(ApiPermission.User)]
	public async Task<IActionResult> GetAccessTokenRefresh(string token) => 
		await GetRefreshAccessTokenAsync(token);
	
	private async Task<IActionResult> GetAccessTokenAndUserAsync(string id)
	{
		JwtToken token;
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
			
			token = JwtGenerator.GenerateUserToken(id.Encryption());
		}
		catch
		{
			return this.ToPublishCode(1);
		}
		
		// 기존 사용자 탐색
		var userAsync = await FirebaseDatabase.GetUserAsync(id);
		if (!string.IsNullOrEmpty(userAsync.Information.Id))
		{
			user = userAsync;
			user.Log.LastConnected = $"{DateTime.UtcNow}";
			
			// 마지막 로그인 기록 갱신
			await FirebaseDatabase.SetUserAsync(id, user);
			await RedisDatabase.SetUserAsync(id, user);

			return new Dictionary<string, object>
			{
				{"user", user},
				{"token", token}
			}.ToJsonPublish();
		}
		
		// 새로운 사용자 데이터 저장
		await FirebaseDatabase.SetUserAsync(id, user);
		await RedisDatabase.SetUserAsync(id, user);
		
		return new Dictionary<string, object>
		{
			{"user", user},
			{"token", token}
		}.ToJsonPublish();
	}

	private Task<IActionResult> GetRefreshAccessTokenAsync(string refreshToken)
	{
		var completionSource = new TaskCompletionSource<IActionResult>();
		completionSource.SetResult(AppToken.JwtTokens.ContainsKey(refreshToken)
			                           ? JwtGenerator.RegenerateUserToken(this.GetUserId(), refreshToken).ToJsonPublish()
			                           : this.ToPublishCode(1));

		return completionSource.Task;
	}
}