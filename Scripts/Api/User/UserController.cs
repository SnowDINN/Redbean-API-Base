using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
	[HttpPost, HttpAuthorize(Role.User)]
	public async Task<IActionResult> PostUserNickname([FromBody] StringRequest requestBody) => 
		await PostUserNicknameAsync(requestBody.Value);

	[HttpPost, HttpAuthorize(Role.User)]
	public async Task<IActionResult> PostUserWithdrawal() => 
		await PostUserWithdrawalAsync();

	private async Task<IActionResult> PostUserNicknameAsync(string nickname)
	{
		var userId = Authorization.GetUserId(Request);
		var user = await Request.GetRequestUser();
		user.Information.Nickname = nickname;
		await Redis.SetUserAsync(userId, user);
		
		await FirebaseSetting.UserCollection?.Document(userId)?.SetAsync(user.ToDocument());
		return this.ToPublishCode();
	}

	private async Task<IActionResult> PostUserWithdrawalAsync()
	{
		var userId = Authorization.GetUserId(Request);
		await FirebaseSetting.Authentication?.DeleteUserAsync(userId);
		await FirebaseSetting.UserCollection?.Document(userId).DeleteAsync();

		return this.ToPublishCode();
	}

	private async Task<IActionResult> PostUserPushNotificationAsync(Message message)
	{
		await FirebaseSetting.Messaging?.SendAsync(message);
		return this.ToPublishCode();
	}
}