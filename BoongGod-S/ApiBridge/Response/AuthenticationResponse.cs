using Newtonsoft.Json;

namespace Redbean.Api
{
#region User

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

#endregion

#region Token

	public class TokenResponse : IApiResponse
	{
		public string AccessToken = "";
		public string RefreshToken = "";
		public DateTime AccessTokenExpire = new();
		public DateTime RefreshTokenExpire = new();
	}	

#endregion
	
	public class UserAndTokenResponse : IApiResponse
	{
		[JsonProperty("user")]
		public UserResponse User = new();

		[JsonProperty("token")]
		public TokenResponse Token = new();
	}
}