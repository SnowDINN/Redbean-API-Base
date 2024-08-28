using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;

namespace Redbean.Firebase;

public class FirebaseSetting
{
	public static string Id { get; set; }
	public static string StorageBucket { get; set; }
	
	public static FirebaseAuth Authentication => FirebaseAuth.DefaultInstance;
	public static FirebaseMessaging Messaging => FirebaseMessaging.DefaultInstance;
}