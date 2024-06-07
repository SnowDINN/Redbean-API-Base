using Google.Cloud.Firestore;

namespace Redbean.Firebase;

public class FirebaseBootstrap
{
	public static async void Setup()
	{
		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", $@"{Directory.GetCurrentDirectory()}\Environment\Firebase\DEV.json");
		
		FirebaseSetting.Firestore = await FirestoreDb.CreateAsync();
	}
}

public class FirebaseSetting
{
	public static FirestoreDb? Firestore { get; set; }
}