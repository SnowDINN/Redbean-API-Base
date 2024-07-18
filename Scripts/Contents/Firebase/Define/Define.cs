using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;

namespace Redbean.Firebase;

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