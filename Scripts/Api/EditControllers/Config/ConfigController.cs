using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ConfigController : ControllerBase
{
	/// <summary>
	/// 테이블 구성 데이터
	/// </summary>
	[HttpGet, HttpSchema(typeof(TableConfigResponse)), HttpAuthorize(Role.Administrator)]
	public async Task<IActionResult> GetTableConfig() => 
		await GetTableConfigAsync();
	
	/// <summary>
	/// 앱 업데이트 버전 변경
	/// </summary>
	[HttpPost, HttpSchema(typeof(AppVersionResponse)), HttpAuthorize(Role.Administrator)]
	public async Task<IActionResult> PostAppVersion([FromBody] AppVersionRequest requestBody) => 
		await PostVersionAsync(requestBody.Type, requestBody.Version);

	private async Task<IActionResult> GetTableConfigAsync()
	{
		var appConfigResponse = await Redis.GetValueAsync<TableConfigResponse>(RedisKey.TABLE_CONFIG);
		return appConfigResponse.ToPublish();
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
		
		await FirebaseSetting.AppConfigDocument?.SetAsync(appConfigResponse.ToDocument());
		return appVersionResponse.ToPublish();
	}
}