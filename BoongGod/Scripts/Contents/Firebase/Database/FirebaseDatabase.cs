using Google.Cloud.Firestore;
using Redbean.Api;
using Redbean.Extension;

namespace Redbean.Firebase;

public class FirebaseDatabase
{
	private static FirestoreDb db;
	
	public static void Initialize(FirestoreDb db) => FirebaseDatabase.db = db;

	public static DocumentReference GetConfigDocument(string document) => 
		db.Collection("config").Document(document);
	
	public static async Task SetAppSettingAsync(object obj) =>
		await db.Collection("config").Document("app").SetAsync(obj);
	
	public static async Task SetTableSettingAsync(object obj) =>
		await db.Collection("config").Document("table").SetAsync(obj);

	public static async Task<UserResponse> GetGuestUserAsync(string id)
	{
		var user = await db.Collection("account_guest").Document(id).GetSnapshotAsync();
		return user.Exists ? user.ToDictionary().ToConvert<UserResponse>() : default;
	}
	
	public static async Task SetGuestUserAsync(string id, UserResponse user) =>
		await db.Collection("account_guest").Document(id).SetAsync(user.ToDocument());
	
	public static async Task DeleteGuestUserAsync(string id) =>
		await db.Collection("account_guest").Document(id).DeleteAsync();
	
	public static async Task<UserResponse> GetUserAsync(string id)
	{
		var user = await db.Collection("account").Document(id).GetSnapshotAsync();
		return user.Exists ? user.ToDictionary().ToConvert<UserResponse>() : default;
	}
	
	public static async Task SetUserAsync(string id, UserResponse user) =>
		await db.Collection("account").Document(id).SetAsync(user.ToDocument());
	
	public static async Task DeleteUserAsync(string id) =>
		await db.Collection("account").Document(id).DeleteAsync();
}