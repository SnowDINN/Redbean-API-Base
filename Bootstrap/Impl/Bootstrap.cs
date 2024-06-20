namespace Redbean;

public class Bootstrap
{
	public static async Task Setup()
	{
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
			.ToArray();

		foreach (var bootstrap in bootstraps)
			await bootstrap.Setup();
	}
}