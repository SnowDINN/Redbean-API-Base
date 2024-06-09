using Microsoft.AspNetCore.Mvc;
using Redbean.Firebase;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ConfigController : ControllerBase
{
	[HttpGet]
	public async Task<string> GetApplicationConfig() => await ConnectConfig("app");
	
	[HttpGet]
	public async Task<string> GetTableConfig() => await ConnectConfig("table");

	private async Task<string> ConnectConfig(string path)
	{
		var document = FirebaseSetting.Firestore.Collection("config").Document(path);
		var snapshot = await document.GetSnapshotAsync();
		if (snapshot.Exists)
		{
			var user = snapshot.ConvertTo<Dictionary<string, object>>();
			return ResponseConvert.ToJson(user);
		}

		return ResponseConvert.ToJson("Config not found", 1);
	}
}