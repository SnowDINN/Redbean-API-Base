using Newtonsoft.Json;
using Redbean.JWT;
using Redbean.Swagger;

namespace Redbean;

public class AppEnvironment
{
	[JsonIgnore]
	public static AppEnvironment Default { get; set; }
	
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

public class AppToken
{
	/// <summary>
	/// Swagger Session Tokens
	/// </summary>
	public static readonly Dictionary<string, SwaggerSessionToken> SwaggerSessionTokens = new();
	
	/// <summary>
	/// JWT Tokens
	/// </summary>
	public static readonly Dictionary<string, JwtToken> JwtTokens = new();
}