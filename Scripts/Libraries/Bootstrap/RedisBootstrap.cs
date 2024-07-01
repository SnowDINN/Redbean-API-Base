using System.Text;
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
		
		FirebaseSetting.AppConfigDocument.Listen(async _ =>
		{
			await Redis.SetValueAsync(RedisKey.APP_CONFIG, _.ToDictionary());
		}).Subscribe(listeners);
		
		FirebaseSetting.TableConfigDocument.Listen(async _ =>
		{
			var table = new Dictionary<string, string>();
			var objects = FirebaseSetting.Storage?.ListObjects(FirebaseSetting.StorageBucket, "Table/");
			foreach (var obj in objects)
			{
				using var memoryStream = new MemoryStream();
				var tableFile = await FirebaseSetting.Storage?.DownloadObjectAsync(obj, memoryStream);

				var fileName = tableFile.Name.Split('/').Last();
				var tableName = fileName.Split('.').First();
			
				table.Add(tableName, Encoding.UTF8.GetString(memoryStream.ToArray()));
			}
			
			await Redis.SetValueAsync(RedisKey.TABLE, table);
			await Redis.SetValueAsync(RedisKey.TABLE_CONFIG, _.ToDictionary());
		}).Subscribe(listeners);
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
		Multiplexer = await ConnectionMultiplexer.ConnectAsync("host.docker.internal:6379");
	}

	/// <summary>
	/// Redis 값 호출
	/// </summary>
	public static async Task<T> GetValueAsync<T>(string key) where T : IApiResponse =>
		JsonConvert.DeserializeObject<T>(await db?.StringGetAsync(key));

	/// <summary>
	/// Redis 값 설정
	/// </summary>
	public static async Task SetValueAsync(string key, object value) => 
		await db?.StringSetAsync(key, JsonConvert.SerializeObject(value));

	/// <summary>
	/// Redis 유저 데이터 호출
	/// </summary>
	public static async Task<UserResponse> GetUserAsync(string userId)
	{
		var redis = await db?.StringGetAsync(userId);
		if (string.IsNullOrEmpty(redis))
		{
			var equalTo = FirebaseSetting.UserCollection?.WhereEqualTo("social.id", userId)?.Limit(1);
			var querySnapshot = await equalTo?.GetSnapshotAsync();
			if (querySnapshot.Count != 0)
			{
				var user = querySnapshot.Documents[0].ToDictionary().ToConvert<UserResponse>();
				await SetUserAsync(user);
			}
		}
		
		return JsonConvert.DeserializeObject<UserResponse>(await db?.StringGetAsync(userId));
	}
	
	/// <summary>
	/// Redis 유저 데이터 설정
	/// </summary>
	public static async Task SetUserAsync(UserResponse user) => 
		await db?.StringSetAsync(user.Social.Id, JsonConvert.SerializeObject(user), TimeSpan.FromDays(1));
}