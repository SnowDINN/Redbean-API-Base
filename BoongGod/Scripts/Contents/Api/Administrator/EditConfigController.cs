using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;
using Redbean.Firebase;
using Redbean.Redis;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class EditConfigController : ControllerBase
{
	/// <summary>
	/// 앱 업데이트 버전 변경
	/// </summary>
	[HttpPost, HttpSchema(typeof(EmptyResponse)), HttpAuthorize(ApiPermission.Administrator)]
	public async Task<IActionResult> EditAppVersion([FromBody] AppVersionRequest requestBody) => 
		await PostVersionAsync(requestBody.Type, requestBody.Version);
	
	/// <summary>
	/// 앱 점검 설정 변경
	/// </summary>
	[HttpPost, HttpSchema(typeof(EmptyResponse)), HttpAuthorize(ApiPermission.Administrator)]
	public async Task<IActionResult> EditAppMaintenance([FromBody] AppMaintenanceRequest requestBody) => 
		await PostAppMaintenanceAsync(requestBody.Contents, requestBody.StartTime, requestBody.EndTime);
	
	private async Task<IActionResult> PostVersionAsync(MobileType type, string version)
	{
		var appConfigResponse = await RedisDatabase.GetValueAsync<AppConfigResponse>(RedisKey.APP_CONFIG);
		
		switch (type)
		{
			case MobileType.Android:
				appConfigResponse.Version.AndroidVersion = version;
				break;
			
			case MobileType.iOS:
				appConfigResponse.Version.iOSVersion = version;
				break;
		}
		
		await FirebaseDatabase.SetAppSettingAsync(appConfigResponse.ToDocument());
		return this.ToPublishCode();
	}

	private async Task<IActionResult> PostAppMaintenanceAsync(string contents, DateTime startTime, DateTime endTime)
	{
		var appConfigResponse = await RedisDatabase.GetValueAsync<AppConfigResponse>(RedisKey.APP_CONFIG);
		appConfigResponse.Maintenance.Contents = contents;
		appConfigResponse.Maintenance.Time.StartTime = $"{startTime}";
		appConfigResponse.Maintenance.Time.EndTime = $"{endTime}";
		
		await FirebaseDatabase.SetAppSettingAsync(appConfigResponse.ToDocument());
		return this.ToPublishCode();
	}
}