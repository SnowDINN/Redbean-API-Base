using Newtonsoft.Json;

namespace Redbean.Api
{
#region AppConfig
	
	public class AppConfigResponse : IApiResponse
	{
		[JsonProperty("maintenance")]
		public AppMaintenanceConfig Maintenance = new();
		
		[JsonProperty("version")]
		public VersionConfig Version = new();
	}

	public class AppMaintenanceConfig
	{
		[JsonProperty("contents")]
		public string Contents = string.Empty;

		[JsonProperty("time")]
		public AppMaintenanceTime Time = new();
	}

	public class AppMaintenanceTime
	{
		[JsonProperty("startTime")]
		public string StartTime = string.Empty;
		
		[JsonProperty("endTime")]
		public string EndTime = string.Empty;
	}
	
	public class VersionConfig
	{
		[JsonProperty("aos_version")]
		public string AndroidVersion = string.Empty;
		
		[JsonProperty("ios_version")]
		public string iOSVersion = string.Empty;
	}

#endregion
}