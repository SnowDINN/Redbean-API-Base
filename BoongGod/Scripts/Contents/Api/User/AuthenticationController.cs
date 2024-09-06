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
	public async Task<IActionResult> PostAccessTokenAndUser([FromBody] UserRequest requestBody) => 
		await PostAccessTokenAndUserAsync(requestBody);
	
	/// <summary>
	/// 리프레시 토큰을 통한 재발급
	/// </summary>
	[HttpPost, HttpSchema(typeof(TokenResponse)), HttpAuthorize(ApiPermission.User)]
	public async Task<IActionResult> PostAccessTokenRefresh([FromBody] StringRequest requestBody) => 
		await PostRefreshAccessTokenAsync(requestBody.Value);
	
	private async Task<IActionResult> PostAccessTokenAndUserAsync(UserRequest requestBody)
	{
		var user = new UserResponse();

		requestBody.id = requestBody.id.Decryption();
		if (requestBody.type == AuthenticationType.Guest)
		{
			// 기존 사용자 탐색
			var findGuestUser = await FirebaseDatabase.GetGuestUserAsync(requestBody.id);
			if (findGuestUser is null)
			{
				// 신규 사용자 데이터 저장
				user.Information.Id = "";
				user.Information.Nickname = "User";

				user.Social.Platform = $"{AuthenticationType.Guest}";
			}
			else
				user = findGuestUser;
		}
		else
		{
			// 기존 사용자 탐색
			var findUser = await FirebaseDatabase.GetUserAsync(requestBody.id);
			if (findUser is null)
			{
				// 신규 사용자 데이터 저장
				try
				{
					var userRecord = await FirebaseSetting.Authentication?.GetUserAsync(requestBody.id);
					if (userRecord.ProviderData.Length > 0)
					{
						user.Social.Profile = userRecord.ProviderData[0].PhotoUrl;
						user.Social.Platform = userRecord.ProviderData[0].ProviderId switch
						{
							var platform when platform.Contains("apple") => $"{AuthenticationType.Apple}",
							var platform when platform.Contains("google") => $"{AuthenticationType.Google}",
							_ => $"{AuthenticationType.Google}"
						};
					}
					
					user.Information.Id = userRecord.Uid;
					user.Information.Nickname = userRecord.DisplayName;
				}
				catch
				{
					return this.ToPublishCode(1);
				}
			}
			else
				user = findUser;
		}

		user.Log.LastConnected = $"{DateTime.UtcNow}";
		return await ReturnUserResponse(requestBody, user);
	}

	private async Task<ContentResult> ReturnUserResponse(UserRequest requestBody, UserResponse user)
	{
		if (string.IsNullOrEmpty(user.Information.Id))
		{
			var isDuplication = true;
			while (isDuplication)
			{
				requestBody.id = $"{Guid.NewGuid()}".Replace("-", "");
				var findGuestUser = await FirebaseDatabase.GetGuestUserAsync(requestBody.id);
				if (findGuestUser is not null)
					continue;

				user.Information.Id = requestBody.id;
				isDuplication = false;
			}
		}
		
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
			{"token", JwtGenerator.GenerateUserToken(user.Information.Id.Encryption())}
		}.ToJsonPublish();
	}

	private Task<IActionResult> PostRefreshAccessTokenAsync(string refreshToken)
	{
		var completionSource = new TaskCompletionSource<IActionResult>();
		completionSource.SetResult(AppToken.JwtTokens.ContainsKey(refreshToken)
			                           ? JwtGenerator.RegenerateUserToken(this.GetUserId(), refreshToken).ToJsonPublish()
			                           : this.ToPublishCode(1));

		return completionSource.Task;
	}
}