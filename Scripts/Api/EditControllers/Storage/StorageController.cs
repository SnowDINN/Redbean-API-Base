using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class StorageController : ControllerBase
{
	/// <summary>
	/// 테이블 데이터 업데이트
	/// </summary>
	[HttpPost, HttpSchema(typeof(StringArrayResponse)), HttpAuthorize(Role.Administrator)]
	public async Task<IActionResult> PostTableFiles([FromBody] AppUploadFilesRequest requestBody) => 
		await PostTablesAsync($"Table/{Authorization.GetVersion(Request)}/", requestBody.Files);

	/// <summary>
	/// 번들 데이터 업데이트
	/// </summary>
	[HttpPost, HttpSchema(typeof(StringArrayResponse)), HttpAuthorize(Role.Administrator)]
	public async Task<IActionResult> PostBundleFiles([FromBody] AppUploadFilesRequest requestBody) => 
		await PostFilesAsync($"Bundle/{Authorization.GetVersion(Request)}/{requestBody.Type}/", requestBody.Files);

	private async Task<IActionResult> PostTablesAsync(string path, IEnumerable<RequestFile> files)
	{
		var tableConfigResponse = await Redis.GetValueAsync<TableConfigResponse>(RedisKey.TABLE_CONFIG);
		tableConfigResponse.UpdateTime = DateTime.UtcNow;
		
		await FirebaseSetting.TableConfigDocument?.SetAsync(tableConfigResponse.ToDocument());
		
		return await PostFilesAsync(path, files);
	}
	
	private async Task<IActionResult> PostFilesAsync(string path, IEnumerable<RequestFile> files)
	{
		await DeleteFiles(path);
		
		foreach (var file in files)
		{
			var obj = new Object
			{
				Bucket = FirebaseSetting.StorageBucket,
				Name = $"{path}{file.FileName}",
				CacheControl = "no-store",
			};

			using var stream = new MemoryStream(file.FileData);
			await FirebaseSetting.Storage?.UploadObjectAsync(obj, stream);	
		}
		
		return new StringArrayResponse(files.Select(_ => _.FileName)).ToPublish();
	}
	
	private async Task DeleteFiles(string path)
	{
		var objects = FirebaseSetting.Storage?.ListObjects(FirebaseSetting.StorageBucket, path);
		var objectList = objects?.Select(obj => obj.Name).ToList();
		foreach (var obj in objectList)
			await FirebaseSetting.Storage?.DeleteObjectAsync($"{FirebaseSetting.Id}.appspot.com", obj);
	}
}