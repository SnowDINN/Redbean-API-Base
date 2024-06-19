using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Redbean.Firebase;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ConfigController : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult> GetAppConfig() => await GetConfigAsync("app");
	
	[HttpGet, ApiAuthorize(Role.Administrator)]
	public async Task<ActionResult> GetTableConfig() => await GetConfigAsync("table");
	
	[HttpPost, ApiAuthorize(Role.Administrator)]
	public async Task<ActionResult> PostAppVersion(string version, int type) => await PostVersionAsync((MobileType)type, version);

	private async Task<ActionResult> GetConfigAsync(string path)
	{
		var document = FirebaseSetting.Firestore?.Collection("config").Document(path)!;
		var snapshot = await document.GetSnapshotAsync()!;
		if (snapshot.Exists)
			return Ok(snapshot.ToDictionary().ToResponse());

		return Ok("Config not found".ToResponse(ApiErrorType.NotExist));
	}
	
	private async Task<ActionResult> PostVersionAsync(MobileType type, string version)
	{
		var key = $"{type}".ToLower() + ".version";
		
		var document = FirebaseSetting.Firestore?.Collection("config").Document("app");
		var snapshotAsync = await document?.GetSnapshotAsync()!;
		
		var response = new AppVersionResponse
		{
			BeforeVersion = snapshotAsync.GetValue<string>(key),
			AfterVersion = version
		};
		
		await document.UpdateAsync(key, version);
		
		return Ok(response.ToResponse());
	}
}