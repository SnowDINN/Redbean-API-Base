using System.Text;
using Microsoft.AspNetCore.Mvc;
using Redbean.Firebase;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class StorageController : ControllerBase
{
	[HttpGet]
	public Task<string> GetTable(string version) => GetTableAsync($"Table/{version}/");
	
	[HttpPost]
	public Task<string> PostTableFiles(string version, IFormFile[] tables) => PostFilesAsync($"Table/{version}/", tables);

	[HttpPost]
	public Task<string> PostBundleFiles(string version, int type, IFormFile[] bundles) => PostFilesAsync($"Bundle/{version}/{(MobileType)type}/", bundles);

	private async Task<string> GetTableAsync(string path)
	{
		var dictionary = new Dictionary<string, object>();
		
		var objects = FirebaseSetting.Storage?.ListObjects(FirebaseSetting.StorageBucket, path)!;
		foreach (var obj in objects)
		{
			using var memoryStream = new MemoryStream();
			var table = await FirebaseSetting.Storage?.DownloadObjectAsync(obj, memoryStream)!;

			var fileName = table.Name.Split('/').Last();
			var tableName = fileName.Split('.').First();
			
			dictionary.Add(tableName, Encoding.UTF8.GetString(memoryStream.ToArray()));
		}

		return dictionary.ToJson();
	}
	
	private Task<string> GetFiles(string path)
	{
		var completionSource = new TaskCompletionSource<string>();
		
		var objects = FirebaseSetting.Storage?.ListObjects(FirebaseSetting.StorageBucket, path)!;
		var objectList = objects?.Select(obj => obj.Name).ToList()!;
		if (objectList != null)
			completionSource.SetResult(objectList.ToJson());

		return completionSource.Task;
	}

	private async Task<string> PostFilesAsync(string path, IEnumerable<IFormFile> files)
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

			await FirebaseSetting.Storage?.UploadObjectAsync(obj, file.OpenReadStream())!;	
		}
		
		return files.Select(_ => _.FileName).ToList().ToJson();
	}
	
	private async Task<string> DeleteFiles(string path)
	{
		var objects = FirebaseSetting.Storage?.ListObjects(FirebaseSetting.StorageBucket, path)!;
		var objectList = objects?.Select(obj => obj.Name).ToList()!;
		foreach (var obj in objectList)
			await FirebaseSetting.Storage?.DeleteObjectAsync($"{FirebaseSetting.Id}.appspot.com", obj)!;
		
		return objectList.ToJson();
	}
}