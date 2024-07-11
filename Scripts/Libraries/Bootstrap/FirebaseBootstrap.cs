﻿using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json.Linq;
using Redbean.Api;

namespace Redbean;

public class FirebaseBootstrap : IBootstrap
{
	public int ExecutionOrder => 10;

	public async Task Setup()
	{
		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", EnvironmentSettings.Default.GoogleCloud.Path);

		using (var reader = new StreamReader(EnvironmentSettings.Default.GoogleCloud.Path))
		{
			var json = JObject.Parse(await reader.ReadToEndAsync());

			FirebaseSetting.Id = json["project_id"]?.Value<string>();
			FirebaseSetting.StorageBucket = $"{FirebaseSetting.Id}.appspot.com";
		}
		
		FirebaseSetting.Firestore = await FirestoreDb.CreateAsync(FirebaseSetting.Id);
		FirebaseSetting.Storage = await StorageClient.CreateAsync();
		
		FirebaseApp.Create();
	}

	public void Dispose()
	{
		FirebaseSetting.Storage?.Dispose();
		
		GC.SuppressFinalize(this);
	}
}

public class FirebaseSetting
{
	public static string Id { get; set; }
	public static string StorageBucket { get; set; }
	
	public static FirebaseAuth Authentication => FirebaseAuth.DefaultInstance;
	public static FirebaseMessaging Messaging => FirebaseMessaging.DefaultInstance;
	public static FirestoreDb Firestore { get; set; }
	public static StorageClient Storage { get; set; }

	public static CollectionReference UserCollection => Firestore.Collection("users");

	public static DocumentReference AppConfigDocument => Firestore.Collection("config").Document("app");
	public static DocumentReference TableConfigDocument => Firestore.Collection("config").Document("table");
}