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

public class FirebaseKey
{
	public const string APP = "app";
	public const string CONFIG = "config";
	public const string TABLE = "table";
	
	public const string ACCOUNT = "account";
	public const string ACCOUNT_GUEST = "account_guest";
}