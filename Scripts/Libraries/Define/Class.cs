using Newtonsoft.Json;

namespace Redbean.Api;

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

public class AuthorizationBody
{
	public string UserId { get; set; } = "";
	public string Role { get; set; } = "";
}

public class MiddlewareMetadata
{
	public DateTime Expire;
	public bool isAuthentication;
}