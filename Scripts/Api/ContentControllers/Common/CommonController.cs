using System.Text;
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
	[HttpGet, HttpSchema(typeof(DictionaryResponse)), HttpAuthorize(Role.User)]
	public Task<IActionResult> GetTable() =>
		GetTableAsync($"Table/{Authorization.GetVersion(Request)}/");


	private async Task<IActionResult> GetAppConfigAsync() =>
		(await Redis.GetValueAsync<AppConfigResponse>(RedisKey.APP_CONFIG)).ToPublish();

	private async Task<IActionResult> GetTableAsync(string path)
	{
		var dictionary = new Dictionary<string, object>();
		
		var objects = FirebaseSetting.Storage?.ListObjects(FirebaseSetting.StorageBucket, path);
		foreach (var obj in objects)
		{
			using var memoryStream = new MemoryStream();
			var table = await FirebaseSetting.Storage?.DownloadObjectAsync(obj, memoryStream);

			var fileName = table.Name.Split('/').Last();
			var tableName = fileName.Split('.').First();
			
			dictionary.Add(tableName, Encoding.UTF8.GetString(memoryStream.ToArray()));
		}

		return new DictionaryResponse(dictionary).ToPublish();
	}
}