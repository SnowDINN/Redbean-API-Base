using System;
using Newtonsoft.Json;

namespace Redbean.Api
{
	public class UserAndTokenResponse : IApiResponse
	{
		[JsonProperty("user")]
		public UserResponse User = new();

		[JsonProperty("token")]
		public TokenResponse Token = new();
	}	
}