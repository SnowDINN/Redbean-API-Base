using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;
using Redbean.Firebase;
using Redbean.JWT;
using Redbean.Redis;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthenticationController : ControllerBase
{
	/// <summary>
	/// 사용자 로그인 및 토큰 발급
	/// </summary>
	[HttpPost, HttpSchema(typeof(UserAndTokenResponse))]
	public async Task<IActionResult> GetAccessTokenAndUser([FromBody] AuthenticationRequest requestBody) => 
		await GetAccessTokenAndUserAsync(requestBody);
	
	/// <summary>
	/// 리프레시 토큰을 통한 재발급
	/// </summary>
	[HttpPost, HttpSchema(typeof(TokenResponse)), HttpAuthorize(ApiPermission.User)]
	public async Task<IActionResult> GetAccessTokenRefresh([FromBody] string token) => 
		await GetRefreshAccessTokenAsync(token);
	
	private async Task<IActionResult> GetAccessTokenAndUserAsync(AuthenticationRequest requestBody)
	{
		var user = new UserResponse();
		var token = JwtGenerator.GenerateUserToken(requestBody.id.Encryption());
		
		requestBody.id = requestBody.id.Decryption();
		if (requestBody.type == AuthenticationType.Guest)
		{
			// 기존 사용자 탐색
			var findGuestUser = await FirebaseDatabase.GetGuestUserAsync(requestBody.id);
			
			// 새로운 사용자 데이터 저장
			if (string.IsNullOrEmpty(findGuestUser.Information.Id))
				return await ReturnUserResponse(requestBody, user, token);

			// 마지막 로그인 기록 갱신
			user = findGuestUser;
			user.Log.LastConnected = $"{DateTime.UtcNow}";

			return await ReturnUserResponse(requestBody, user, token);
		}

		// 사용자 유효성 검사
		try
		{
			var userRecord = await FirebaseSetting.Authentication?.GetUserAsync(requestBody.id);
			user.Information.Id = userRecord.Uid.Encryption();
			user.Information.Nickname = userRecord.DisplayName;
			user.Log.LastConnected = $"{DateTime.UtcNow}";
			
			if (userRecord.ProviderData.Length > 0)
			{
				user.Social.Profile = userRecord.ProviderData[0].PhotoUrl;
				user.Social.Platform = userRecord.ProviderData[0].ProviderId;
			}
		}
		catch
		{
			return this.ToPublishCode(1);
		}
		
		// 기존 사용자 탐색
		var findUser = await FirebaseDatabase.GetUserAsync(requestBody.id);
		
		// 새로운 사용자 데이터 저장
		if (string.IsNullOrEmpty(findUser.Information.Id))
			return await ReturnUserResponse(requestBody, user, token);

		// 마지막 로그인 기록 갱신
		user = findUser;
		user.Log.LastConnected = $"{DateTime.UtcNow}";

		return await ReturnUserResponse(requestBody, user, token);
	}

	private async Task<ContentResult> ReturnUserResponse(AuthenticationRequest requestBody, UserResponse user, JwtToken token)
	{
		switch (requestBody.type)
		{
			case AuthenticationType.Guest:
			{
				await FirebaseDatabase.SetGuestUserAsync(requestBody.id, user);
				break;
			}

			case AuthenticationType.Google:
			case AuthenticationType.Apple:
			{
				await FirebaseDatabase.SetUserAsync(requestBody.id, user);
				break;
			}
		}
		
		await RedisDatabase.SetUserAsync(requestBody.id, user);

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