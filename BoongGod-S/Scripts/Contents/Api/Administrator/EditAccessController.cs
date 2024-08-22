using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;
using Redbean.JWT;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class EditAccessController : ControllerBase
{
	/// <summary>
	/// 에디터 전용 토큰 발급
	/// </summary>
	[HttpPost, HttpSchema(typeof(StringResponse))]
	public async Task<IActionResult> EditAppAccessToken([FromBody] StringRequest requestBody) =>
		await PostEditorAccessTokenAsync(requestBody.Value);
	
	private Task<IActionResult> PostEditorAccessTokenAsync(string email)
	{
		var completionSource = new TaskCompletionSource<IActionResult>();
		
		// 사용자 유효성 검사
		try
		{
			email = email.Decryption();
			completionSource.SetResult(ApiPermission.AdministratorEmails.Contains(email)
				                           ? new StringResponse(JwtGenerator.GenerateAdministratorTokenAsync()).ToPublish()
				                           : this.ToPublishCode(1));
		}
		catch
		{
			completionSource.SetResult(this.ToPublishCode(1));
		}

		return completionSource.Task;
	}
}