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
	private const long ExpiresTime = 10;
	
	private const string userKey = "user";
	private const string tokenKey = "token";

	[HttpGet]
	public async Task<Response> GetToken(string uid)
	{
		try
		{
			var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
			
			if (App.AdministratorEmail.Contains(userRecord.Email))
				return GetTokenAsync(uid, userRecord.Email).ToResponse();
		}
		catch (FirebaseAuthException)
		{
			return "User not found".ToResponse(ApiErrorType.NotExist);
		}

		return "User not found".ToResponse(ApiErrorType.NotExist);
	}
	
	[HttpGet]
	public async Task<Response> GetUser(string uid)
	{
		UserResponse? user;
		string? email;
		
		try
		{
			var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
			user = new UserResponse
			{
				Social =
				{
					Id = userRecord.Uid,
					Platform = userRecord.ProviderData[0].ProviderId
				},
				Information =
				{
					Nickname = userRecord.ProviderData[0].DisplayName
				}
			};

			email = userRecord.Email;
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
				{ userKey, querySnapshot.Documents[0].ToDictionary() },
				{ tokenKey, GetTokenAsync(uid, email) }
			}.ToResponse();
		
		var document = FirebaseSetting.Firestore?.Collection("users").Document(uid);
		await document?.SetAsync(user.ToDocument())!;

		return new Dictionary<string, object>
		{
			{ userKey, user.ToResponse() },
			{ tokenKey, GetTokenAsync(uid, email) }
		}.ToResponse();
	}
	
	private AccessTokenResponse GetTokenAsync(string uid, string email)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var token = new JwtSecurityToken(expires: DateTime.UtcNow.AddSeconds(ExpiresTime),
		                                 claims: new[]
		                                 {
			                                 new Claim(ClaimTypes.NameIdentifier, uid),
			                                 new Claim(ClaimTypes.Email, email),
			                                 new Claim(ClaimTypes.Role, App.AdministratorEmail.Contains(email) ? Role.Administrator : Role.User)
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