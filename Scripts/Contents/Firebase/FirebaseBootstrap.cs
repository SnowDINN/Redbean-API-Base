using FirebaseAdmin;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json.Linq;
using Redbean.Firebase;

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