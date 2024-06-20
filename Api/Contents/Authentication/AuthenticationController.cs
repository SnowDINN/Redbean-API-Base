#pragma warning disable CS8602

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Web;
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
	private const long AccessTokenExpireSecond = 600;
	private const long RefreshTokenExpireSecond = 600;
	
	private const string userKey = "user";
	private const string tokenKey = "token";

	[HttpGet]
	public async Task<ActionResult> GetToken(string uid)
	{
		try
		{
			var userId = HttpUtility.UrlDecode(uid.Decrypt());
			var userRecord = await FirebaseAuth.DefaultInstance?.GetUserAsync(userId);
			
			if (App.AdministratorKey.Contains(userRecord.Email))
				return Ok(GetTokenAsync(userId, userRecord.Email).ToResponse());
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
		
		var userId = HttpUtility.UrlDecode(uid.Decrypt());
		
		try
		{
			var userRecord = await FirebaseAuth.DefaultInstance?.GetUserAsync(userId);
			user = new UserResponse
			{
				Social =
				{
					Id = userRecord.Uid.Encrypt(),
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
		
		var equalTo = FirebaseSetting.Firestore?.Collection("users").WhereEqualTo("social.id", userId);
		var querySnapshot = await equalTo?.GetSnapshotAsync();
		if (querySnapshot.Count != 0)
			return Ok(new Dictionary<string, object>
			{
				{ userKey, querySnapshot.Documents[0].ToDictionary() },
				{ tokenKey, GetTokenAsync(userId, email) }
			}.ToResponse());
		
		var document = FirebaseSetting.Firestore?.Collection("users").Document(userId);
		await document?.SetAsync(user.ToDocument());

		return Ok(new Dictionary<string, object>
		{
			{ userKey, user.ToResponse() },
			{ tokenKey, GetTokenAsync(userId, email) }
		}.ToResponse());
	}
	
	private TokenResponse GetTokenAsync(string uid, string email)
	{
		var accessTokenExpire = DateTime.UtcNow.AddSeconds(AccessTokenExpireSecond);
		var refreshTokenExpire = DateTime.UtcNow.AddSeconds(RefreshTokenExpireSecond);
		
		var accessToken = new JwtSecurityToken(expires: accessTokenExpire,
		                                 claims: new[]
		                                 {
			                                 new Claim(ClaimTypes.NameIdentifier, uid.Encrypt()),
			                                 new Claim(ClaimTypes.Role, App.AdministratorKey.Contains(email) ? Role.Administrator : Role.User)
		                                 },
		                                 signingCredentials: new SigningCredentials(new SymmetricSecurityKey(App.SecurityKey), SecurityAlgorithms.HmacSha256));
		var refreshToken = $"{Guid.NewGuid()}".Replace("-", "");
		
		App.RefreshTokens[uid] = new TokenResponse
		{
			AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
			RefreshToken = refreshToken,
			AccessTokenExpire = accessTokenExpire,
			RefreshTokenExpire = refreshTokenExpire,
		};
		return App.RefreshTokens[uid];
	}
}