using FirebaseAdmin;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json.Linq;
using Redbean.Firebase.Storage;

namespace Redbean.Firebase;

public class FirebaseBootstrap : IBootstrap
{
	public int ExecutionOrder => 10;

	public async Task Setup()
	{
		using (var reader = new StreamReader(AppEnvironment.Default.GoogleCloud.Path))
		{
			var json = JObject.Parse(await reader.ReadToEndAsync());

			FirebaseSetting.Id = json["project_id"]?.Value<string>();
			FirebaseSetting.StorageBucket = $"{FirebaseSetting.Id}.appspot.com";
		}
		
		FirebaseDatabase.Initialize(await FirestoreDb.CreateAsync(FirebaseSetting.Id));
		FirebaseStorage.Initialize(await StorageClient.CreateAsync());
		
		FirebaseApp.Create();
	}
}