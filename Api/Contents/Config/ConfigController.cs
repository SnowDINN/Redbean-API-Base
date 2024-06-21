#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8604

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Redbean.Extension;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ConfigController : ControllerBase
{
	[HttpGet]
	public async Task<IActionResult> GetAppConfig() =>
		Content(await Redis.GetValueAsync(RedisKey.APP_CONFIG), ContentType.Json);
	
	[HttpGet, ApiAuthorize(Role.Administrator)]
	public async Task<IActionResult> GetTableConfig() => 
		Content(await Redis.GetValueAsync(RedisKey.TABLE_CONFIG), ContentType.Json);
	
	[HttpPost, ApiAuthorize(Role.Administrator)]
	public async Task<ActionResult> PostAppVersion(string version, int type) => 
		await PostVersionAsync((MobileType)type, version);
	
	private async Task<ActionResult> PostVersionAsync(MobileType type, string version)
	{
		var redis = await Redis.GetValueAsync(RedisKey.APP_CONFIG);
		var appConfigResponse = redis.ToConvert<AppConfigResponse>();
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
		return Ok(appVersionResponse.ToResponse());
	}
}