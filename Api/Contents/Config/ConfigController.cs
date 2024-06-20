#pragma warning disable CS8602

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ConfigController : ControllerBase
{
	[HttpGet]
	public async Task<IActionResult> GetAppConfig() =>
		Content(await Redis.GetValue(RedisKey.APP_CONFIG), ContentType.Json);
	
	[HttpGet, ApiAuthorize(Role.Administrator)]
	public async Task<IActionResult> GetTableConfig() => 
		Content(await Redis.GetValue(RedisKey.TABLE_CONFIG), ContentType.Json);
	
	[HttpPost, ApiAuthorize(Role.Administrator)]
	public async Task<ActionResult> PostAppVersion(string version, int type) => 
		await PostVersionAsync((MobileType)type, version);
	
	private async Task<ActionResult> PostVersionAsync(MobileType type, string version)
	{
		var key = $"value.{type}".ToLower() + ".version";
		
		var value = await Redis.GetValue(RedisKey.APP_CONFIG);
		var beforeVersion = JObject.Parse(value).SelectToken(key).Value<string>();
		
		var response = new AppVersionResponse
		{
			BeforeVersion = beforeVersion,
			AfterVersion = version
		};

		await FirebaseSetting.Firestore?.Collection("config").Document("app").UpdateAsync(key, version);
		
		return Ok(response.ToResponse());
	}
}