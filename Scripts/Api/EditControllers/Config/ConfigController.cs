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
	
	/// <summary>
	/// 앱 점검 설정 변경
	/// </summary>
	[HttpPost, HttpSchema(typeof(AppConfigResponse))]
	public async Task<IActionResult> PostAppMaintenance([FromBody] AppMaintenanceRequest requestBody) => 
		await PostAppMaintenanceAsync(requestBody.Contents, requestBody.StartTime, requestBody.EndTime);
	
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

	private async Task<IActionResult> PostAppMaintenanceAsync(string contents, DateTime startTime, DateTime endTime)
	{
		var appConfigResponse = await Redis.GetValueAsync<AppConfigResponse>(RedisKey.APP_CONFIG);
		appConfigResponse.Maintenance.Contents = contents;
		appConfigResponse.Maintenance.Time.StartTime = $"{startTime.Hour}:{startTime.Minute}";
		appConfigResponse.Maintenance.Time.EndTime = $"{endTime.Hour}:{endTime.Minute}";
		
		await FirebaseSetting.AppConfigDocument?.SetAsync(appConfigResponse.ToDocument());
		return appConfigResponse.ToPublish();
	}
}