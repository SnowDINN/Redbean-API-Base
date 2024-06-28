using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class CommonController : ControllerBase
{
	/// <summary>
	/// 앱 구성 데이터
	/// </summary>
	[HttpGet]
	public async Task<AppConfigResponse> GetAppConfig() =>
		await GetAppConfigAsync();
	
	/// <summary>
	/// 테이블 데이터
	/// </summary>
	[HttpGet, ApiAuthorize(Role.User)]
	public Task<DictionaryResponse> GetTable() =>
		GetTableAsync($"Table/{Authorization.GetVersion(Request)}/");
	
	
	private async Task<AppConfigResponse> GetAppConfigAsync() =>
		await Redis.GetValueAsync<AppConfigResponse>(RedisKey.APP_CONFIG);
	
	private async Task<DictionaryResponse> GetTableAsync(string path)
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

		return new DictionaryResponse(dictionary);
	}
}