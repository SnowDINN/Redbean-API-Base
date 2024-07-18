using Newtonsoft.Json;
using Redbean.Api;
using Redbean.Extension;
using Redbean.Firebase;
using StackExchange.Redis;

namespace Redbean.Redis;

public class RedisContainer
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
				await SetUserAsync(userId, user);
			}
		}
    		
		return JsonConvert.DeserializeObject<UserResponse>(await db?.StringGetAsync(userId));
	}
    	
	/// <summary>
	/// Redis 유저 데이터 설정
	/// </summary>
	public static async Task SetUserAsync(string userId, UserResponse user) => 
		await db?.StringSetAsync(userId, JsonConvert.SerializeObject(user), TimeSpan.FromDays(1));
}