using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
	[HttpPost, HttpSchema(typeof(UserResponse)), HttpAuthorize(Role.User)]
	public async Task<IActionResult> PostUserNickname([FromBody] StringRequest requestBody) => 
		await PostUserNicknameAsync(requestBody.Value);

	private async Task<IActionResult> PostUserNicknameAsync(string nickname)
	{
		var user = await Request.GetRequestUser();

		user.Information.Nickname = nickname;
		await Redis.SetUserAsync(user);
		
		await FirebaseSetting.UserCollection?.Document(user.Social.Id)?.SetAsync(user.ToDocument());
		return user.ToPublish();
	}
}