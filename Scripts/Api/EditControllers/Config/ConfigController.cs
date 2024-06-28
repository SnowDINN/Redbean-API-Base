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
	[HttpGet, ApiAuthorize(Role.Administrator)]
	public async Task<TableConfigResponse> GetTableConfig() => 
		await GetTableConfigAsync();
	
	/// <summary>
	/// 앱 업데이트 버전 변경
	/// </summary>
	[HttpPost, ApiAuthorize(Role.Administrator)]
	public async Task<AppVersionResponse> PostAppVersion(string version, int type) => 
		await PostVersionAsync((MobileType)type, version);

	private async Task<TableConfigResponse> GetTableConfigAsync()
	{
		var appConfigResponse = await Redis.GetValueAsync<TableConfigResponse>(RedisKey.TABLE_CONFIG);
		return appConfigResponse;
	}
	
	private async Task<AppVersionResponse> PostVersionAsync(MobileType type, string version)
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
		return appVersionResponse;
	}
}