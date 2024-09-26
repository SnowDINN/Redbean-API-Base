using System.Net;
using System.Text;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Redbean.Extension;
using Redbean.Firebase;
using Redbean.JWT;
using Redbean.Redis;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
	[HttpPost, HttpSchema(typeof(EmptyResponse)), HttpAuthorize(ApiPermission.User)]
	public async Task<IActionResult> PostUserNickname([FromBody] StringRequest requestBody) => 
		await PostUserNicknameAsync(requestBody.Value);

	[HttpPost, HttpSchema(typeof(EmptyResponse)), HttpAuthorize(ApiPermission.User)]
	public async Task<IActionResult> PostUserWithdrawal([FromBody] UserWithdrawalRequest requestBody) => 
		await PostUserWithdrawalAsync(requestBody);

	private async Task<IActionResult> PostUserNicknameAsync(string nickname)
	{
		var user = await this.GetUser();
		user.Information.Nickname = nickname;
		
		var userId = this.GetUserId().Decryption();
		await RedisDatabase.SetUserAsync(userId, user);
		await FirebaseDatabase.SetUserAsync(userId, user);
		
		return this.ToPublishCode();
	}

	private async Task<IActionResult> PostUserWithdrawalAsync(UserWithdrawalRequest requestBody)
	{
		var userId = this.GetUserId().Decryption();
		await FirebaseDatabase.DeleteUserAsync(userId);

		if (requestBody.type == AuthenticationType.Guest)
			await FirebaseDatabase.DeleteGuestUserAsync(userId);
		else
		{
			await FirebaseSetting.Authentication?.DeleteUserAsync(userId);
			await FirebaseDatabase.DeleteUserAsync(userId);
		}

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