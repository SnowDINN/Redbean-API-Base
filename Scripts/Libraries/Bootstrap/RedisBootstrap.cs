﻿using Google.Cloud.Firestore;
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
		}).Subscribe(listeners);
		
		collection.Document("table").Listen(async _ =>
		{
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
		Multiplexer = await ConnectionMultiplexer.ConnectAsync("localhost");
	}

	/// <summary>
	/// Redis 값 호출
	/// </summary>
	public static async Task<T> GetValueAsync<T>(string key) where T : IResponse =>
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
			var equalTo = FirebaseSetting.Firestore?.Collection("users").WhereEqualTo("social.id", userId).Limit(1);
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
	public static async Task SetUserAsync(string userId, object value) => 
		await db?.StringSetAsync(userId, JsonConvert.SerializeObject(value), TimeSpan.FromDays(1));
}