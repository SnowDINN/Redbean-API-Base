using Newtonsoft.Json;

namespace Redbean;

public class AppSequenceBootstrap : IBootstrap
{
	public int ExecutionOrder => 0;
	
	public async Task Setup()
	{
		JwtPermission.SetAdministrators(ApiPermission.AdministratorEmails.ToList());
		JwtPermission.SetPermission(PermissionType.Administrator, ApiPermission.Administrator);
		JwtPermission.SetPermission(PermissionType.User, ApiPermission.User);
		
		var environmentPath = "";
#if REDBEAN_RELEASE
		environmentPath = "Environment/Release/Settings.json";
#else
		environmentPath = "Environment/Develop/Settings.json";
#endif
		using var stream = new StreamReader(environmentPath);
		AppEnvironment.Default =
			JsonConvert.DeserializeObject<AppEnvironment>(await stream.ReadToEndAsync());
		
		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", AppEnvironment.Default.GoogleCloud.Path);
	}
}