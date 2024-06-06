using Microsoft.AspNetCore.Mvc;

namespace BoongGod.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
	[HttpGet("[action]")]
	public async Task<string> Register(string id = "Guest")
	{
		await Task.Delay(TimeSpan.FromSeconds(2.5f));
		return ResponseConvert.ToResult($"{id} register the boongGod");
	}
}

public abstract class AuthenticationApi
{
	private const string Controller = "Authentication";
	
	public static void Setup(WebApplication app)
	{
		app.MapGet($"{Controller}/Login", (string id) =>
			{
				
				if (int.TryParse(id, out var integer))
					return ResponseConvert.ToResult($"not support number", 1);

				return ResponseConvert.ToResult($"{id} login the boongGod");
			})
			.WithTags(Controller)
			.WithOpenApi();
	}
}