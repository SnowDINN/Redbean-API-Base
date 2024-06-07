using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Redbean.Firebase;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
	[HttpGet("[action]")]
	public async Task<string> Register(string id = "Guest")
	{
		await Task.Delay(TimeSpan.FromSeconds(2.5f));
		return ResponseConvert.ToJson($"{id} register the boongGod");
	}
	
	[HttpGet("[action]")]
	public async Task<string> GetUser(string id = "Guest")
	{

		var docRef = FirebaseSetting.Firestore.Collection("users").Document(id);
		var snapshot = await docRef.GetSnapshotAsync();
		if (snapshot.Exists)
		{
			var user = snapshot.ConvertTo<Dictionary<string, object>>();
			return ResponseConvert.ToJson(user);
		}

		return ResponseConvert.ToJson("User not found", 404);
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
					return ResponseConvert.ToJson($"not support number", 1);

				return ResponseConvert.ToJson($"{id} login the boongGod");
			})
			.WithTags(Controller)
			.WithOpenApi();
	}
}