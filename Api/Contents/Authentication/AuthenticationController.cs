using Microsoft.AspNetCore.Mvc;
using Redbean.Firebase;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
	[HttpGet("[action]")]
	public async Task<string> GetUser(string uid)
	{
		var document = FirebaseSetting.Firestore.Collection("users").Document(uid);
		var snapshot = await document.GetSnapshotAsync();
		if (snapshot.Exists)
		{
			var user = snapshot.ConvertTo<Dictionary<string, object>>();
			return ResponseConvert.ToJson(user);
		}

		return ResponseConvert.ToJson("User not found", 1);
	}
}