using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class CommonController : ControllerBase
{
	/// <summary>
	/// 앱 구성 데이터
	/// </summary>
	[HttpGet, HttpSchema(typeof(AppConfigResponse))]
	public async Task<IActionResult> GetAppConfig() =>
		(await Redis.GetValueAsync<AppConfigResponse>(RedisKey.APP_CONFIG)).ToPublish();
	
	/// <summary>
	/// 테이블 구성 데이터
	/// </summary>
	[HttpGet, HttpSchema(typeof(TableConfigResponse)), HttpAuthorize(Role.Administrator)]
	public async Task<IActionResult> GetTableConfig() => 
		(await Redis.GetValueAsync<TableConfigResponse>(RedisKey.TABLE_CONFIG)).ToPublish();
	
	/// <summary>
	/// 테이블 데이터
	/// </summary>
	[HttpGet, HttpSchema(typeof(TableResponse)), HttpAuthorize(Role.User)]
	public async Task<IActionResult> GetTable() =>
		(await Redis.GetValueAsync<TableResponse>(RedisKey.TABLE)).ToPublish();
}