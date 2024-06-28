namespace Redbean.Api;

public class AuthorizationBody
{
	public string UserId { get; set; } = "";
	public string Role { get; set; } = "";
}

public class AuthenticationState
{
	public DateTime Expire;
	public bool isAuthentication;
}