using Newtonsoft.Json;

namespace Redbean.Api
{
	public class UserResponse : IApiResponse
	{
		[JsonProperty("information")]
		public UserInformation Information = new();

		[JsonProperty("social")]
		public UserSocial Social = new();

		[JsonProperty("log")]
		public UserLog Log = new();
	}
	
	public class UserInformation
	{
		[JsonProperty("id")]
		public string Id = string.Empty;
		
		[JsonProperty("nickname")]
		public string Nickname = string.Empty;
	}
	
	public class UserSocial
	{
		[JsonProperty("profile")]
		public string Profile = string.Empty;
		
		[JsonProperty("platform")]
		public string Platform = string.Empty;
	}

	public class UserLog
	{
		[JsonProperty("lastConnected")]
		public string LastConnected = string.Empty;
		
		[JsonProperty("lastPurchased")]
		public string LastPurchased = string.Empty;
	}
}