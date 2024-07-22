using System.Reflection;

namespace Redbean;

public class Bootstrap
{
	public static async Task Setup(Assembly[] assemblies)
	{
		var bootstraps = assemblies
			.SelectMany(x => x.GetTypes())
			.Where(x => x.FullName != null
			            && typeof(IBootstrap).IsAssignableFrom(x)
			            && x is
			            {
				            IsInterface: false,
				            IsAbstract: false
			            })
			.Select(x => Activator.CreateInstance(x.Assembly.FullName, x.FullName).Unwrap() as IBootstrap)
			.OrderBy(_ => _.ExecutionOrder)
			.ToArray();

		foreach (var bootstrap in bootstraps)
			await bootstrap?.Setup();
	}
}