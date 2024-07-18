using System.Text;

namespace Redbean.Security;

public class AppSecurity
{
	public static readonly byte[] SecurityKey = Encoding.ASCII.GetBytes($"{Guid.NewGuid()}".Replace("-", ""));
}

public class SecurityRole
{
	public static readonly string[] AdministratorEmails = ["mfactory86@gmail.com"];
	
	/// <summary>
	/// [어드민 권한] Redbean.Boongsin.Administrator
	/// </summary>
	public const string Administrator = "4+LIjHPPOByQA/QRXhwOY8hmfCG3QA0XzSbKz0NNTJs=";
	
	/// <summary>
	/// [유저 권한] Redbean.Boongsin.User
	/// </summary>
	public const string User = "4+LIjHPPOByQA/QRXhwOY7s8gH8HxiwDWzk+C0icKxw=";
}