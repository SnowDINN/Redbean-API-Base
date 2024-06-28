using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
	[HttpPost, ApiAuthorize(Role.User)]
	public async Task<UserResponse> PostUserNickname(string nickname) => 
		await PostUserNicknameAsync(nickname);

	private async Task<UserResponse> PostUserNicknameAsync(string nickname)
	{
		var user = await Request.GetRequestUser();

		user.Information.Nickname = nickname;
		await Redis.SetUserAsync(user);
		
		await FirebaseSetting.UserCollection?.Document(user.Social.Id)?.SetAsync(user.ToDocument());
		return user;
	}
}