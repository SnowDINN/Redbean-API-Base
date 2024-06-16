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
	public async Task<ActionResult> GetToken(string uid)
	{
		try
		{
			var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
			
			if (App.AdministratorEmail.Contains(userRecord.Email))
				return Ok(GetTokenAsync(uid, userRecord.Email).ToResponse());
		}
		catch (FirebaseAuthException)
		{
			return BadRequest();
		}

		return BadRequest();
	}
	
	[HttpGet]
	public async Task<ActionResult> GetUser(string uid)
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
			return BadRequest();
		}
		
		var equalTo = FirebaseSetting.Firestore?.Collection("users").WhereEqualTo("social.id", uid);
		var querySnapshot = await equalTo?.GetSnapshotAsync()!;
		if (querySnapshot.Count != 0)
			return Ok(new Dictionary<string, object>
			{
				{ userKey, querySnapshot.Documents[0].ToDictionary() },
				{ tokenKey, GetTokenAsync(uid, email) }
			}.ToResponse());
		
		var document = FirebaseSetting.Firestore?.Collection("users").Document(uid);
		await document?.SetAsync(user.ToDocument())!;

		return Ok(new Dictionary<string, object>
		{
			{ userKey, user.ToResponse() },
			{ tokenKey, GetTokenAsync(uid, email) }
		}.ToResponse());
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