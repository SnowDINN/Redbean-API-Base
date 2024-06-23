using Google.Cloud.Firestore;
using Newtonsoft.Json;
using Redbean.Api;
using Redbean.Extension;
using StackExchange.Redis;
using RedisKey = Redbean.Api.RedisKey;

namespace Redbean;

public class RedisBootstrap : IBootstrap
{
	private readonly List<FirestoreChangeListener> listeners = [];
	public int ExecutionOrder => 20;

	public async Task Setup()
	{
		await Redis.Initialize();
		
		var collection = FirebaseSetting.Firestore?.Collection("config");
		collection.Document("app").Listen(async _ =>
		{
			await Redis.SetValueAsync(RedisKey.APP_CONFIG, _.ToDictionary());
		}).AddListener(listeners);
		
		collection.Document("table").Listen(async _ =>
		{
			await Redis.SetValueAsync(RedisKey.TABLE_CONFIG, _.ToDictionary());
		}).AddListener(listeners);
	}

	public async void Dispose()
	{
		await Redis.Multiplexer.DisposeAsync();
		
		foreach (var listener in listeners)
			await listener.StopAsync();
		
		listeners.Clear();
		
		GC.SuppressFinalize(this);
	}
}

public class Redis
{
	public static ConnectionMultiplexer Multiplexer { get; private set; }
	private static IDatabase db => Multiplexer?.GetDatabase();

	public static async Task Initialize()
	{
		Multiplexer = await ConnectionMultiplexer.ConnectAsync("localhost");
	}

	public static async Task<T> GetValueAsync<T>(string key) where T : IResponse =>
		JsonConvert.DeserializeObject<T>(await db?.StringGetAsync(key));

	public static async Task SetValueAsync(string key, object value) => 
		await db?.StringSetAsync(key, JsonConvert.SerializeObject(value));
	
	public static async Task SetValueAsync(string key, object value, TimeSpan expired) => 
		await db?.StringSetAsync(key, JsonConvert.SerializeObject(value), expired);
}