using Newtonsoft.Json;
using StackExchange.Redis;

namespace Redbean;

public class RedisBootstrap : IBootstrap
{
	public static ConnectionMultiplexer? Redis { get; private set; }
	
	public async Task Setup()
	{
		Redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
	}

	public async void Dispose()
	{
		await Redis.DisposeAsync();
		
		GC.SuppressFinalize(this);
	}
}

public class Redis
{
	private static IDatabase? db => RedisBootstrap.Redis?.GetDatabase();
	
	public static string GetValue(string key) => db.StringGet(key);

	public static void SetValue(string key, object value) => db.StringSet(key, JsonConvert.SerializeObject(value));
}