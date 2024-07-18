using Newtonsoft.Json;
using Redbean.Api;

namespace Redbean;

public class Bootstrap
{
	public static async Task Setup()
	{
		var environmentPath = "";
#if REDBEAN_RELEASE
		environmentPath = "Environment/Release/Settings.json";
#else
		environmentPath = "Environment/Develop/Settings.json";
#endif
		
		using (var stream = new StreamReader(environmentPath))
			EnvironmentSettings.Default =
				JsonConvert.DeserializeObject<EnvironmentSettings>(await stream.ReadToEndAsync());
		
		var bootstraps = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(x => x.GetTypes())
			.Where(x => x.FullName != null
			            && typeof(IBootstrap).IsAssignableFrom(x)
			            && x is
			            {
				            IsInterface: false,
				            IsAbstract: false
			            })
			.Select(x => Activator.CreateInstance(Type.GetType(x.FullName)) as IBootstrap)
			.OrderBy(_ => _.ExecutionOrder)
			.ToArray();

		foreach (var bootstrap in bootstraps)
			await bootstrap?.Setup();
	}
}