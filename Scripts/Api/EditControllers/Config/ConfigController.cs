using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ConfigController : ControllerBase
{
	/// <summary>
	/// 앱 업데이트 버전 변경
	/// </summary>
	[HttpPost, HttpSchema(typeof(AppVersionResponse)), HttpAuthorize(Role.Administrator)]
	public async Task<IActionResult> PostAppVersion([FromBody] AppVersionRequest requestBody) => 
		await PostVersionAsync(requestBody.Type, requestBody.Version);
	
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
				appVersionResponse.BeforeVersion = appConfigResponse.Version.AndroidVersion;
				appConfigResponse.Version.AndroidVersion = version;
				break;
			
			case MobileType.iOS:
				appVersionResponse.BeforeVersion = appConfigResponse.Version.iOSVersion;
				appConfigResponse.Version.iOSVersion = version;
				break;
		}
		
		await FirebaseSetting.AppConfigDocument?.SetAsync(appConfigResponse.ToDocument());
		return appVersionResponse.ToPublish();
	}
}