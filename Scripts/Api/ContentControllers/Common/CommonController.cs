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
		await GetAppConfigAsync();
	
	/// <summary>
	/// 테이블 데이터
	/// </summary>
	[HttpGet, HttpSchema(typeof(TableResponse)), HttpAuthorize(Role.User)]
	public async Task<IActionResult> GetTable() =>
		await GetTableAsync();


	private async Task<IActionResult> GetAppConfigAsync() =>
		(await Redis.GetValueAsync<AppConfigResponse>(RedisKey.APP_CONFIG)).ToPublish();

	private async Task<IActionResult> GetTableAsync() =>
		(await Redis.GetValueAsync<TableResponse>(RedisKey.TABLE)).ToPublish();
}