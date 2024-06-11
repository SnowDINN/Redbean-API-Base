using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;
using Redbean.Firebase;

namespace Redbean.Api.Controllers;

public enum PlatformType
{
	Android,
	iOS
}

[ApiController]
[Route("[controller]/[action]")]
public class ConfigController : ControllerBase
{
	[HttpGet]
	public async Task<string> GetApplicationConfig() => await GetConfig("app");
	
	[HttpGet]
	public async Task<string> GetTableConfig() => await GetConfig("table");
	
	[HttpPost]
	public async Task<string> PostAndroidVersion(string version) => await PostVersion(PlatformType.Android, version);
	
	[HttpPost]
	public async Task<string> PostiOSVersion(string version) => await PostVersion(PlatformType.iOS, version);

	private async Task<string> GetConfig(string path)
	{
		var document = FirebaseSetting.Firestore?.Collection("config").Document(path)!;
		var snapshot = await document.GetSnapshotAsync()!;
		if (snapshot.Exists)
			return snapshot.ToDictionary().ToJson();

		return "Config not found".ToJson(1);
	}
	
	private async Task<string> PostVersion(PlatformType type, string version)
	{
		var document = FirebaseSetting.Firestore?.Collection("config").Document("app")!;
		var snapshot = await document.GetSnapshotAsync()!;
		if (snapshot.Exists)
		{
			var config = snapshot.ToConvert<AppConfigResponse>();
			var response = new AppVersionResponse();
			
			switch (type)
			{
				case PlatformType.Android:
					response.BeforeVersion = config?.Android.Version!;
					response.AfterVersion = version;
					if (config != null)
						config.Android.Version = version;
					break;
				
				case PlatformType.iOS:
					response.BeforeVersion = config?.iOS.Version!;
					response.AfterVersion = version;
					if (config != null)
						config.iOS.Version = version;
					break;
			}
			
			await document.SetAsync(config.ToDocument());
			
			return response.ToJson();
		}

		return "Config not found".ToJson(1);
	}
}