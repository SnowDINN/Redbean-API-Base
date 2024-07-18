using System.Net;
using System.Text;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Redbean.Extension;
using Redbean.Firebase;
using Redbean.JWT;
using Redbean.Redis;
using Redbean.Security;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
	[HttpPost, HttpAuthorize(SecurityRole.User)]
	public async Task<IActionResult> PostUserNickname([FromBody] StringRequest requestBody) => 
		await PostUserNicknameAsync(requestBody.Value);

	[HttpPost, HttpAuthorize(SecurityRole.User)]
	public async Task<IActionResult> PostUserWithdrawal() => 
		await PostUserWithdrawalAsync();

	private async Task<IActionResult> PostUserNicknameAsync(string nickname)
	{
		var userId = this.GetUserId().Decryption();
		var user = await this.GetUser();
		user.Information.Nickname = nickname;
		await RedisContainer.SetUserAsync(userId, user);
		
		await FirebaseSetting.UserCollection?.Document(userId)?.SetAsync(user.ToDocument());
		return this.ToPublishCode();
	}

	private async Task<IActionResult> PostUserWithdrawalAsync()
	{
		var userId = this.GetUserId().Decryption();
		await FirebaseSetting.Authentication?.DeleteUserAsync(userId);
		await FirebaseSetting.UserCollection?.Document(userId).DeleteAsync();

		return this.ToPublishCode();
	}

	private async Task<IActionResult> PostUserPushNotificationAsync(Message message)
	{
		await FirebaseSetting.Messaging?.SendAsync(message);
		return this.ToPublishCode();
	}

	private async Task<string> GetAppleAccessCode(string clientSecret, string authorizationCode)
	{
		var http = new HttpClient
		{
			DefaultRequestHeaders =
			{
				{ "content-type", "application/x-www-form-urlencoded" }
			}
		};
		
		var request = await http.PostAsync("https://appleid.apple.com/auth/token",
		                                   new StringContent($"grant_type=authorization_code&client_id={string.Empty /* need application identifier */}&client_secret={clientSecret}&code={authorizationCode}",
		                                                     Encoding.UTF8,
		                                                     "application/x-www-form-urlencoded"));
		
		if (request.StatusCode != HttpStatusCode.OK)
			return string.Empty;

		var jObject = JObject.Parse(await request.Content.ReadAsStringAsync());
		return jObject["access_token"].Value<string>();
	}

	private async Task<bool> AppleRevokeAccount(string clientSecret, string accessToken)
	{
		var http = new HttpClient
		{
			DefaultRequestHeaders =
			{
				{ "content-type", "application/x-www-form-urlencoded" }
			}
		};
		
		var request = await http.PostAsync("https://appleid.apple.com/auth/revoke",
		                                   new StringContent($"client_id={string.Empty /* need application identifier */}&client_secret={clientSecret}&token={accessToken}",
		                                                     Encoding.UTF8,
		                                                     "application/x-www-form-urlencoded"));

		return request.StatusCode == HttpStatusCode.OK;
	}
}