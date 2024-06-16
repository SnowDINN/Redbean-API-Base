using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;
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
		var document = FirebaseSetting.Firestore?.Collection("config").Document("app")!;
		var snapshot = await document.GetSnapshotAsync()!;
		if (snapshot.Exists)
		{
			var config = snapshot.ToConvert<AppConfigResponse>();
			var response = new AppVersionResponse();
			
			switch (type)
			{
				case MobileType.Android:
					response.BeforeVersion = config?.Android.Version!;
					response.AfterVersion = version;
					if (config != null)
						config.Android.Version = version;
					break;
				
				case MobileType.iOS:
					response.BeforeVersion = config?.iOS.Version!;
					response.AfterVersion = version;
					if (config != null)
						config.iOS.Version = version;
					break;
			}
			
			await document.SetAsync(config.ToDocument());
			
			return Ok(response.ToResponse());
		}

		return Ok("Config not found".ToResponse(ApiErrorType.NotExist));
	}
}