using Microsoft.AspNetCore.Mvc;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class StorageController : ControllerBase
{
	/// <summary>
	/// 테이블 데이터 업데이트
	/// </summary>
	[HttpPost, ApiAuthorize(Role.Administrator)]
	public Task<StringArrayResponse> PostTableFiles(IFormFile[] tables) => 
		PostFilesAsync($"Table/{Authorization.GetVersion(Request)}/", tables);

	/// <summary>
	/// 번들 데이터 업데이트
	/// </summary>
	[HttpPost, ApiAuthorize(Role.Administrator)]
	public Task<StringArrayResponse> PostBundleFiles(int type, IFormFile[] bundles) => 
		PostFilesAsync($"Bundle/{Authorization.GetVersion(Request)}/{(MobileType)type}/", bundles);

	private async Task<StringArrayResponse> PostFilesAsync(string path, IEnumerable<IFormFile> files)
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
		
		return new StringArrayResponse(files.Select(_ => _.FileName));
	}
	
	private async Task DeleteFiles(string path)
	{
		var objects = FirebaseSetting.Storage?.ListObjects(FirebaseSetting.StorageBucket, path);
		var objectList = objects?.Select(obj => obj.Name).ToList();
		foreach (var obj in objectList)
			await FirebaseSetting.Storage?.DeleteObjectAsync($"{FirebaseSetting.Id}.appspot.com", obj);
	}
}