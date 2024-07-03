using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Redbean.Extension;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class EditAccessController : ControllerBase
{
	/// <summary>
	/// 에디터 전용 토큰 발급
	/// </summary>
	[HttpPost, HttpSchema(typeof(StringResponse))]
	public async Task<IActionResult> PostEditorAccessToken([FromBody] StringRequest requestBody) =>
		await PostEditorAccessTokenAsync(requestBody.Value);
	
	private Task<IActionResult> PostEditorAccessTokenAsync(string email)
	{
		var completionSource = new TaskCompletionSource<IActionResult>();
		
		// 사용자 유효성 검사
		try
		{
			email = email.Decryption();
			completionSource.SetResult(Authorization.Administrators.Contains(email)
				                           ? new StringResponse(GenerateAdministratorTokenAsync()).ToPublish()
				                           : this.ToPublishCode(1));
		}
		catch
		{
			completionSource.SetResult(this.ToPublishCode(1));
		}

		return completionSource.Task;
	}
	
	private string GenerateAdministratorTokenAsync()
	{
		var accessToken = new JwtSecurityToken(expires: DateTime.UtcNow.AddSeconds(30),
		                                       claims: new[]
		                                       {
			                                       new Claim(ClaimTypes.Role, Role.Administrator)
		                                       },
		                                       signingCredentials: new SigningCredentials(new SymmetricSecurityKey(AppSecurity.SecurityKey), SecurityAlgorithms.HmacSha256));
		
		return new JwtSecurityTokenHandler().WriteToken(accessToken);
	}
}