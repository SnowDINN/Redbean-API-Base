using Microsoft.AspNetCore.Mvc;
using Redbean.Firebase;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class StorageController : ControllerBase
{
	[HttpGet]
	public Task<string> GetTables(string version) => GetFiles($"Table/{version}/");
	
	[HttpGet]
	public Task<string> GetAndroidBundles(string version) => GetFiles($"Bundle/{version}/Android/");
	
	[HttpGet]
	public Task<string> GetiOSBundles(string version) => GetFiles($"Bundle/{version}/iOS/");

	[HttpDelete]
	public async Task<string> DeleteTables(string version) => await DeleteFiles($"Table/{version}/");
	
	[HttpDelete]
	public async Task<string> DeleteAndroidBundles(string version) => await DeleteFiles($"Bundle/{version}/iOS/");
	
	[HttpDelete]
	public async Task<string> DeleteiOSBundles(string version) => await DeleteFiles($"Bundle/{version}/iOS/");

	private Task<string> GetFiles(string path)
	{
		var taskCompletionSource = new TaskCompletionSource<string>();
		
		var objects = FirebaseSetting.Storage.ListObjects(FirebaseSetting.StorageBucket, path);
		var list = objects.Select(obj => obj.Name).ToList();
		
		taskCompletionSource.SetResult(ResponseConvert.ToJson(list));
		return taskCompletionSource.Task;
	}
	
	private async Task<string> DeleteFiles(string path)
	{
		var objects = FirebaseSetting.Storage.ListObjects(FirebaseSetting.StorageBucket, path);
		var list = objects.Select(obj => obj.Name).ToList();
		foreach (var obj in list)
			await FirebaseSetting.Storage.DeleteObjectAsync($"{FirebaseSetting.Id}.appspot.com", obj);
		
		return ResponseConvert.ToJson(list);
	}
}