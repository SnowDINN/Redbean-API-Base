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
	[HttpGet, HttpSchema(typeof(AppConfigResponse))]
	public async Task<IActionResult> GetAppConfig() =>
		(await RedisDatabase.GetValueAsync<AppConfigResponse>(RedisKey.APP_CONFIG)).ToPublish();
	
	/// <summary>
	/// 테이블 구성 데이터
	/// </summary>
	[HttpGet, HttpSchema(typeof(TableConfigResponse))]
	public async Task<IActionResult> GetTableConfig() => 
		(await RedisDatabase.GetValueAsync<TableConfigResponse>(RedisKey.TABLE_CONFIG)).ToPublish();
	
	/// <summary>
	/// 테이블 데이터
	/// </summary>
	[HttpGet, HttpSchema(typeof(TableResponse)), HttpAuthorize(ApiPermission.User)]
	public async Task<IActionResult> GetTable() =>
		(await RedisDatabase.GetValueAsync<TableResponse>(RedisKey.TABLE)).ToPublish();
}