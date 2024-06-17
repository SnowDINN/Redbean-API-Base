﻿using System.IdentityModel.Tokens.Jwt;
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
	private const long ExpiresTime = 10;
	
	private const string userKey = "user";
	private const string tokenKey = "token";

	[HttpGet]
	public async Task<ActionResult> GetToken(string uid)
	{
		try
		{
			var userId = HttpUtility.UrlDecode(uid.Decrypt());
			var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(userId);
			
			if (App.AdministratorEmail.Contains(userRecord.Email))
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
			var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(userId);
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
		var querySnapshot = await equalTo?.GetSnapshotAsync()!;
		if (querySnapshot.Count != 0)
			return Ok(new Dictionary<string, object>
			{
				{ userKey, querySnapshot.Documents[0].ToDictionary() },
				{ tokenKey, GetTokenAsync(userId, email) }
			}.ToResponse());
		
		var document = FirebaseSetting.Firestore?.Collection("users").Document(userId);
		await document?.SetAsync(user.ToDocument())!;

		return Ok(new Dictionary<string, object>
		{
			{ userKey, user.ToResponse() },
			{ tokenKey, GetTokenAsync(userId, email) }
		}.ToResponse());
	}
	
	private AccessTokenResponse GetTokenAsync(string uid, string email)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var token = new JwtSecurityToken(expires: DateTime.UtcNow.AddSeconds(ExpiresTime),
		                                 claims: new[]
		                                 {
			                                 new Claim(ClaimTypes.NameIdentifier, uid.Encrypt()),
			                                 new Claim(ClaimTypes.Email, email.Encrypt()),
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