using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Redbean.Extension;
using Redbean.Firebase;

namespace Redbean.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthenticationController : ControllerBase
{
	private const long ExpiresTime = 1800;
	
	[HttpGet]
	public async Task<Response> GetUser(string uid, string version)
	{
		const string userKey = "user";
		const string tokenKey = "token";
		
		if (uid == App.AdministratorCode)
			return new Dictionary<string, object>
			{
				{ userKey, null! },
				{ tokenKey, GetAccessToken(uid, version) }
			}.ToResponse();

		UserResponse? user;
		try
		{
			var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
			user = new UserResponse
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
		}
		catch (FirebaseAuthException)
		{
			return new Dictionary<string, object>
			{
				{ userKey, null! },
				{ tokenKey, null! }
			}!.ToResponse(ApiErrorType.NotExist);
		}
		
		var equalTo = FirebaseSetting.Firestore?.Collection("users").WhereEqualTo("social.id", uid);
		var querySnapshot = await equalTo?.GetSnapshotAsync()!;
		if (querySnapshot.Count != 0)
			return  new Dictionary<string, object>
			{
				{ userKey, querySnapshot.Documents.First().ToDictionary() },
				{ tokenKey, GetAccessToken(uid, version) }
			}.ToResponse();
		
		var document = FirebaseSetting.Firestore?.Collection("users").Document(uid);
		await document?.SetAsync(user.ToDocument())!;

		return new Dictionary<string, object>
		{
			{ userKey, user.ToResponse() },
			{ tokenKey, GetAccessToken(uid, version) }
		}.ToResponse();
	}
	
	private AccessTokenResponse GetAccessToken(string uid, string version)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var token = new JwtSecurityToken(expires: DateTime.UtcNow.AddSeconds(ExpiresTime),
		                                 claims: new[]
		                                 {
			                                 new Claim(ClaimTypes.NameIdentifier, uid),
			                                 new Claim(ClaimTypes.Version, version),
			                                 new Claim(ClaimTypes.Role, uid == App.AdministratorCode ? Role.Administrator : Role.User)
		                                 },
		                                 signingCredentials: new SigningCredentials(new SymmetricSecurityKey(App.SecurityKey), SecurityAlgorithms.HmacSha256Signature));

		var tokenResponse = new AccessTokenResponse
		{
			AccessToken = tokenHandler.WriteToken(token),
			Expires = ExpiresTime
		};
		
		return tokenResponse;
	}
}