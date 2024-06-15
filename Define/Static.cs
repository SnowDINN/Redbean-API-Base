using System.Text;

namespace Redbean.Api;

public class App
{
	public static byte[] SecurityKey { get; set; } = Encoding.ASCII.GetBytes($"{Guid.NewGuid()}".Replace("-", ""));
	
	public const string AdministratorCode = "redbean.boongsin.admin";
}

public class Role
{
	public const string Administrator = "Redbean.Boongsin.Administrator";
	public const string User = "Redbean.Boongsin.User";
}