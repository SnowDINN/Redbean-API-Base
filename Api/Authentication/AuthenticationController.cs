using Microsoft.AspNetCore.Mvc;

namespace BoongGod.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
	[HttpGet("[action]")]
	public async Task<string> Register(string id)
	{
		await Task.CompletedTask;
		return id;
	}
}

public abstract class AuthenticationApi
{
	private const string Controller = "Authentication";
	
	public static void Setup(WebApplication app)
	{
		app.MapGet($"{Controller}/Login", (string id) =>
			{
				return id;
			})
			.WithTags(Controller)
			.WithOpenApi();
	}
}