﻿#pragma warning disable CS8602

using System.Text;
using Microsoft.AspNetCore.Mvc;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class StorageController : ControllerBase
{
	[HttpGet, ApiAuthorize(Role.Administrator, Role.User)]
	public Task<ActionResult> GetTable(string version) => GetTableAsync($"Table/{version}/");
	
	[HttpPost, ApiAuthorize(Role.Administrator)]
	public Task<ActionResult> PostTableFiles(string version, IFormFile[] tables) => PostFilesAsync($"Table/{version}/", tables);

	[HttpPost, ApiAuthorize(Role.Administrator)]
	public Task<ActionResult> PostBundleFiles(string version, int type, IFormFile[] bundles) => PostFilesAsync($"Bundle/{version}/{(MobileType)type}/", bundles);

	private async Task<ActionResult> GetTableAsync(string path)
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

		return Ok(dictionary.ToResponse());
	}

	private async Task<ActionResult> PostFilesAsync(string path, IEnumerable<IFormFile> files)
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

			await FirebaseSetting.Storage?.UploadObjectAsync(obj, file.OpenReadStream());	
		}
		
		return Ok(files.Select(_ => _.FileName).ToList().ToResponse());
	}
	
	private async Task DeleteFiles(string path)
	{
		var objects = FirebaseSetting.Storage?.ListObjects(FirebaseSetting.StorageBucket, path);
		var objectList = objects?.Select(obj => obj.Name).ToList();
		foreach (var obj in objectList)
			await FirebaseSetting.Storage?.DeleteObjectAsync($"{FirebaseSetting.Id}.appspot.com", obj);
	}
}