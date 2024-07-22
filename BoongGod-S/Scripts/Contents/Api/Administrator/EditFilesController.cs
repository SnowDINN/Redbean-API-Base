using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;
using Redbean.Firebase;
using Redbean.Firebase.Storage;
using Redbean.JWT;
using Redbean.Redis;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class EditFilesController : ControllerBase
{
	/// <summary>
	/// 테이블 데이터 업데이트
	/// </summary>
	[HttpPost, HttpSchema(typeof(StringArrayResponse)), HttpAuthorize(ApiPermission.Administrator)]
	public async Task<IActionResult> PostTableFiles([FromBody] AppUploadFilesRequest requestBody) => 
		await PostTablesAsync($"Table/", requestBody.Files);

	/// <summary>
	/// 번들 데이터 업데이트
	/// </summary>
	[HttpPost, HttpSchema(typeof(StringArrayResponse)), HttpAuthorize(ApiPermission.Administrator)]
	public async Task<IActionResult> PostBundleFiles([FromBody] AppUploadFilesRequest requestBody) => 
		await PostFilesAsync($"Bundle/{this.GetVersion()}/{requestBody.Type}/", requestBody.Files);

	private async Task<IActionResult> PostTablesAsync(string path, IEnumerable<RequestFile> files)
	{
		var tableUploadRequest = await PostFilesAsync(path, files);
		var tableConfigResponse = await RedisDatabase.GetValueAsync<TableConfigResponse>(RedisKey.TABLE_CONFIG);
		tableConfigResponse.Update.UpdateTime = $"{DateTime.UtcNow}";
		
		await FirebaseDatabase.SetTableSettingAsync(tableConfigResponse.ToDocument());
		return tableUploadRequest
	;
    	}
	private async Task<IActionResult> PostFilesAsync(string path, IEnumerable<RequestFile> files)
	{
		await FirebaseStorage.DeleteFilesAsync(FirebaseStorage.GetFiles(path));
		await FirebaseStorage.UploadFilesAsync(path, files);
		
		return new StringArrayResponse(files.Select(_ => _.FileName)).ToPublish();
	}
}