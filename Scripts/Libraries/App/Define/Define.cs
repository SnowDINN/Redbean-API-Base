using Newtonsoft.Json;
using Redbean.JWT;
using Redbean.Middleware;

namespace Redbean;

public class EnvironmentSettings
{
	[JsonIgnore]
	public static EnvironmentSettings Default { get; set; }
	
	public SwaggerSettings Swagger { get; set; }
	public GoogleCloudSettings GoogleCloud { get; set; }
}

public class SwaggerSettings
{
	public string OauthClientId { get; set; }
	public string OauthClientSecretId { get; set; }
}

public class GoogleCloudSettings
{
	public string Path { get; set; }
}

public class GoogleAuthentication
{
	/// <summary>
	/// Google Middleware Tokens
	/// </summary>
	public static readonly Dictionary<string, GoogleMiddleware> Tokens = new();
}

public class JwtAuthentication
{
	/// <summary>
	/// JWT Tokens
	/// </summary>
	public static readonly Dictionary<string, JwtToken> Tokens = new();
}