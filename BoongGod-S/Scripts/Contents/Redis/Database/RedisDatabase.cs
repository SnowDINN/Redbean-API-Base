using Newtonsoft.Json;
using Redbean.Api;
using Redbean.Firebase;
using StackExchange.Redis;

namespace Redbean.Redis;

public class RedisDatabase
{
	private static IDatabase db;
	
	public static void Initialize(IDatabase db) => RedisDatabase.db = db;
	
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
			var user = await FirebaseDatabase.GetUserAsync(userId);
			if (!string.IsNullOrEmpty(user.Information.Id))
				await SetUserAsync(userId, user);
		}
    		
		return JsonConvert.DeserializeObject<UserResponse>(await db?.StringGetAsync(userId));
	}
    	
	/// <summary>
	/// Redis 유저 데이터 설정
	/// </summary>
	public static async Task SetUserAsync(string userId, UserResponse user) => 
		await db?.StringSetAsync(userId, JsonConvert.SerializeObject(user), TimeSpan.FromDays(1));
}