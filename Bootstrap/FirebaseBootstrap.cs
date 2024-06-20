using FirebaseAdmin;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json.Linq;

namespace Redbean;

public class FirebaseBootstrap : IBootstrap
{
	public async Task Setup()
	{
		var environmentPath = $@"{Directory.GetCurrentDirectory()}\Environment\Firebase\DEV.json";
		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", environmentPath);

		using var reader = new StreamReader(environmentPath);
		var json = JObject.Parse(await reader.ReadToEndAsync());

		FirebaseSetting.Id = json["project_id"]?.Value<string>();
		FirebaseSetting.StorageBucket = $"{FirebaseSetting.Id}.appspot.com";
		
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
	public static string? Id { get; set; }
	public static string? StorageBucket { get; set; }
	
	public static FirestoreDb? Firestore { get; set; }
	public static StorageClient? Storage { get; set; }
}