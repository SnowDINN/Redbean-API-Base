using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Redbean.Api
{
#region Table

	public class TableResponse : IApiResponse
	{
		[JsonProperty("table")]
		public Dictionary<string, string> Table = new();
	}	

#endregion
	
#region TableConfig

	public class TableConfigResponse : IApiResponse
	{
		[JsonProperty("client")]
		public TableClientConfig Client = new();
		
		[JsonProperty("sheet")]
		public TableSheetConfig Sheet = new();

		[JsonProperty("update")]
		public TableUpdateConfig Update = new();
	}
	
	public class TableClientConfig
	{
		[JsonProperty("id")]
		public string Id = string.Empty;
		
		[JsonProperty("secret")]
		public string Secret = string.Empty;
	}
	
	public class TableSheetConfig
	{
		[JsonProperty("id")]
		public string Id = string.Empty;
	}
	
	public class TableUpdateConfig
	{
		[JsonProperty("updateTime")]
		public string UpdateTime = $"{DateTime.UtcNow}";
	}

#endregion
}