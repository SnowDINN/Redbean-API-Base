using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ConfigController : ControllerBase
{
	[HttpGet]
	public async Task<IActionResult> GetAppConfig() =>
		await GetConfigAsync(RedisKey.APP_CONFIG);
	
	[HttpGet, ApiAuthorize(Role.Administrator)]
	public async Task<IActionResult> GetTableConfig() => 
		await GetConfigAsync(RedisKey.TABLE_CONFIG);
	
	[HttpPost, ApiAuthorize(Role.Administrator)]
	public async Task<IActionResult> PostAppVersion(string version, int type) => 
		await PostVersionAsync((MobileType)type, version);

	private async Task<IActionResult> GetConfigAsync(string key)
	{
		var appConfigResponse = await Redis.GetValueAsync<AppConfigResponse>(key);
		return appConfigResponse.ToResponse();
	}
	
	private async Task<IActionResult> PostVersionAsync(MobileType type, string version)
	{
		var appConfigResponse = await Redis.GetValueAsync<AppConfigResponse>(RedisKey.APP_CONFIG);
		var appVersionResponse = new AppVersionResponse
		{
			AfterVersion = version
		};

		switch (type)
		{
			case MobileType.Android:
				appVersionResponse.BeforeVersion = appConfigResponse.Android.Version;
				appConfigResponse.Android.Version = version;
				break;
			
			case MobileType.iOS:
				appVersionResponse.BeforeVersion = appConfigResponse.iOS.Version;
				appConfigResponse.iOS.Version = version;
				break;
		}
		
		await FirebaseSetting.Firestore?.Collection("config").Document("app").SetAsync(appConfigResponse.ToDocument());
		return appVersionResponse.ToResponse();
	}
}