﻿using Google.Cloud.Firestore;
using Redbean.Api;
using Redbean.Extension;

namespace Redbean.Firebase;

public class FirebaseDatabase
{
	private static FirestoreDb db;
	
	public static void Initialize(FirestoreDb db) => FirebaseDatabase.db = db;

	public static DocumentReference GetConfigDocument(string document) => 
		db.Collection(FirebaseKey.CONFIG).Document(document);
	
	public static async Task SetAppSettingAsync(object obj) =>
		await db.Collection(FirebaseKey.CONFIG).Document(FirebaseKey.APP).SetAsync(obj);
	
	public static async Task SetTableSettingAsync(object obj) =>
		await db.Collection(FirebaseKey.CONFIG).Document(FirebaseKey.TABLE).SetAsync(obj);
	
	public static async Task<UserResponse> GetUserAsync(string id)
	{
		var user = await db.Collection(FirebaseKey.ACCOUNT).Document(id).GetSnapshotAsync();
		return user.Exists ? user.ToDictionary().ToConvert<UserResponse>() : default;
	}
	
	public static async Task SetUserAsync(string id, UserResponse user) =>
		await db.Collection(FirebaseKey.ACCOUNT).Document(id).SetAsync(user.ToDocument());
	
	public static async Task DeleteUserAsync(string id) =>
		await db.Collection(FirebaseKey.ACCOUNT).Document(id).DeleteAsync();

	public static async Task<UserResponse> GetGuestUserAsync(string id)
	{
		var user = await db.Collection(FirebaseKey.ACCOUNT_GUEST).Document(id).GetSnapshotAsync();
		return user.Exists ? user.ToDictionary().ToConvert<UserResponse>() : default;
	}
	
	public static async Task SetGuestUserAsync(string id, UserResponse user) =>
		await db.Collection(FirebaseKey.ACCOUNT_GUEST).Document(id).SetAsync(user.ToDocument());
	
	public static async Task DeleteGuestUserAsync(string id) =>
		await db.Collection(FirebaseKey.ACCOUNT_GUEST).Document(id).DeleteAsync();
}