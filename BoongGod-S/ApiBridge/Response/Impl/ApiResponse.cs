using Newtonsoft.Json;

namespace Redbean.Api
{
	public class ApiResponse : IApiResponse
	{
		[JsonIgnore]
		public static ApiResponse Default { get; set; } = new();
		
		[JsonProperty("errorCode")]
		public int ErrorCode { get; set; }

		[JsonProperty("response")]
		public object Response { get; set; }
	}
	
	public class ApiResponse<T> : IApiResponse
	{
		[JsonProperty("errorCode")]
		public int ErrorCode { get; set; }

		[JsonProperty("response")]
		public T Response { get; set; }
	}
}