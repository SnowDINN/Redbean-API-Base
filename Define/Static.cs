using System.Text;

namespace Redbean.Api;

public class App
{
	public static byte[] SecurityKey { get; set; } = Encoding.ASCII.GetBytes($"{Guid.NewGuid()}".Replace("-", ""));

	public static string[] AdministratorEmail { get; set; } = ["mfactory86@gmail.com"];
}

public class Role
{
	public const string Administrator = "Redbean.Boongsin.Administrator";
	public const string User = "Redbean.Boongsin.User";
}