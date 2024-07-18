using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;
using Redbean.Redis;
using Redbean.Security;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class GetSettingController : ControllerBase
{
	/// <summary>
	/// 앱 구성 데이터
	/// </summary>
	[HttpGet, HttpSchema(typeof(AppConfigResponse))]
	public async Task<IActionResult> GetAppConfig() =>
		(await RedisContainer.GetValueAsync<AppConfigResponse>(RedisKey.APP_CONFIG)).ToPublish();
	
	/// <summary>
	/// 테이블 구성 데이터
	/// </summary>
	[HttpGet, HttpSchema(typeof(TableConfigResponse))]
	public async Task<IActionResult> GetTableConfig() => 
		(await RedisContainer.GetValueAsync<TableConfigResponse>(RedisKey.TABLE_CONFIG)).ToPublish();
	
	/// <summary>
	/// 테이블 데이터
	/// </summary>
	[HttpGet, HttpSchema(typeof(TableResponse)), HttpAuthorize(SecurityRole.User)]
	public async Task<IActionResult> GetTable() =>
		(await RedisContainer.GetValueAsync<TableResponse>(RedisKey.TABLE)).ToPublish();
}