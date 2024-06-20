using System.Text;

namespace Redbean.Api;

public class App
{
	public static readonly Dictionary<string, TokenResponse> RefreshTokens = new();
	
	public static readonly byte[] SecurityKey = Encoding.ASCII.GetBytes($"{Guid.NewGuid()}".Replace("-", ""));
	public static readonly string[] AdministratorKey = ["mfactory86@gmail.com"];
}

public class Role
{
	public const string Administrator = "Redbean.Boongsin.Administrator";
	public const string User = "Redbean.Boongsin.User";
}