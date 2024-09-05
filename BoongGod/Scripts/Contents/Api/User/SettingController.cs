using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;
using Redbean.Redis;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SettingController : ControllerBase
{
	/// <summary>
	/// 앱 구성 데이터
	/// </summary>
	[HttpGet, HttpSchema(typeof(AppSettingResponse))]
	public async Task<IActionResult> GetAppSetting() =>
		(await RedisDatabase.GetValueAsync<AppSettingResponse>(RedisKey.APP_CONFIG)).ToPublish();
	
	/// <summary>
	/// 테이블 데이터
	/// </summary>
	[HttpGet, HttpSchema(typeof(TableSettingResponse)), HttpAuthorize(ApiPermission.User)]
	public async Task<IActionResult> GetTableSetting() =>
		(await RedisDatabase.GetValueAsync<TableSettingResponse>(RedisKey.TABLE)).ToPublish();
}