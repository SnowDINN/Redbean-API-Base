#pragma warning disable CS8602
#pragma warning disable CS8603

using Newtonsoft.Json;
using Redbean.Api;
using StackExchange.Redis;
using RedisKey = Redbean.Api.RedisKey;

namespace Redbean;

public class RedisBootstrap : IBootstrap
{
	public int ExecutionOrder => 20;

	public async Task Setup()
	{
		await Redis.Initialize();
		
		var collection = FirebaseSetting.Firestore?.Collection("config");
		collection.Document("app").Listen(async _ =>
		{
			await Redis.SetValue(RedisKey.APP_CONFIG, _.ToDictionary());
		});
		collection.Document("table").Listen(async _ =>
		{
			await Redis.SetValue(RedisKey.TABLE_CONFIG, _.ToDictionary());
		});
	}

	public async void Dispose()
	{
		await Redis.Multiplexer.DisposeAsync();
		
		GC.SuppressFinalize(this);
	}
}

public class Redis
{
	public static ConnectionMultiplexer? Multiplexer { get; private set; }
	private static IDatabase? db => Multiplexer?.GetDatabase();

	public static async Task Initialize()
	{
		Multiplexer = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
	}
	
	public static async Task<string> GetValue(string key) => await db?.StringGetAsync(key);

	public static async Task<string> SetValue(string key, object value) => await db?.StringGetSetAsync(key, JsonConvert.SerializeObject(value));
}