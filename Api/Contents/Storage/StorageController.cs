using Microsoft.AspNetCore.Mvc;
using Redbean.Firebase;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class StorageController : ControllerBase
{
	[HttpGet]
	public Task<string> GetTableFiles(string version) => GetFiles($"Table/{version}/");
	
	[HttpGet]
	public Task<string> GetAndroidBundleFiles(string version) => GetFiles($"Bundle/{version}/Android/");
	
	[HttpGet]
	public Task<string> GetiOSBundleFiles(string version) => GetFiles($"Bundle/{version}/iOS/");
	
	[HttpPost]
	public Task<string> PostTableFiles(string version, IFormFile[] files) => PostFiles($"Table/{version}/", files);

	[HttpPost]
	public Task<string> PostAndroidBundleFiles(string version, IFormFile[] files) => PostFiles($"Bundle/{version}/Android/", files);

	[HttpPost]
	public Task<string> PostiOSBundleFiles(string version, IFormFile[] files) => PostFiles($"Bundle/{version}/iOS/", files);

	[HttpDelete]
	public async Task<string> DeleteTableFiles(string version) => await DeleteFiles($"Table/{version}/");
	
	[HttpDelete]
	public async Task<string> DeleteAndroidBundleFiles(string version) => await DeleteFiles($"Bundle/{version}/Android/");
	
	[HttpDelete]
	public async Task<string> DeleteiOSBundleFiles(string version) => await DeleteFiles($"Bundle/{version}/iOS/");

	private Task<string> GetFiles(string path)
	{
		var completionSource = new TaskCompletionSource<string>();
		
		var objects = FirebaseSetting.Storage?.ListObjects(FirebaseSetting.StorageBucket, path)!;
		var objectList = objects?.Select(obj => obj.Name).ToList()!;
		if (objectList != null)
			completionSource.SetResult(objectList.ToJson());

		return completionSource.Task;
	}

	private async Task<string> PostFiles(string path, IEnumerable<IFormFile> files)
	{
		var fileList = new List<string>();
		
		foreach (var file in files)
		{
			var obj = new Object
			{
				Bucket = FirebaseSetting.StorageBucket,
				Name = $"{path}{file.FileName}",
				ContentType = file.ContentType,
				CacheControl = "no-store",
			};

			await FirebaseSetting.Storage?.UploadObjectAsync(obj, file.OpenReadStream());
			
			fileList.Add(file.FileName);
		}
		
		return fileList.ToJson();
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