using System;

namespace Redbean.Api
{
	public class TokenResponse : IApiResponse
	{
		public string AccessToken = "";
		public string RefreshToken = "";
		public DateTime AccessTokenExpire = new();
		public DateTime RefreshTokenExpire = new();
	}	
}