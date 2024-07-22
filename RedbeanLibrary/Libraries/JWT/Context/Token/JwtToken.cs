namespace Redbean.JWT;

public class JwtToken
{
	public string AccessToken = "";
	public string RefreshToken = "";
	public DateTime AccessTokenExpire = new();
	public DateTime RefreshTokenExpire = new();
}

public class JwtBody
{
	public string UserId { get; set; } = "";
	public string Role { get; set; } = "";
}