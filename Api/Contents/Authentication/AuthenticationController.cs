using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Redbean.Extension;
using Redbean.Firebase;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthenticationController : ControllerBase
{
	[HttpGet]
	public async Task<string> GetUser(string uid)
	{
		var equalTo = FirebaseSetting.Firestore?.Collection("users").WhereEqualTo("social.id", uid);
		var querySnapshot = await equalTo?.GetSnapshotAsync()!;
		if (querySnapshot.Count != 0)
			return querySnapshot.Documents.First().ToDictionary().ToJson();
		
		var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
		var user = new UserResponse
		{
			Social =
			{
				Id = userRecord.Uid,
				Platform = userRecord.ProviderData.First().ProviderId
			},
			Information =
			{
				Nickname = userRecord.ProviderData.First().DisplayName
			}
		};
		
		var document = FirebaseSetting.Firestore?.Collection("users").Document(uid);
		await document?.SetAsync(user.ToDocument())!;
		
		return user.ToJson();
	}
}