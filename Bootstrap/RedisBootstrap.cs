using Newtonsoft.Json;
using StackExchange.Redis;

namespace Redbean;

public class RedisBootstrap
{
	public static void Setup()
	{
		Redis.Initialize(ConnectionMultiplexer.Connect("localhost:6379"));
	}
}

public class Redis
{
	private static ConnectionMultiplexer? redis { get; set; }
	private static IDatabase? db => redis?.GetDatabase();

	public static void Initialize(ConnectionMultiplexer multiplexer)
	{
		redis = multiplexer;
	}
	
	public static string GetValue(string key) => db.StringGet(key);

	public static void SetValue(string key, object value) => db.StringSet(key, JsonConvert.SerializeObject(value));
}