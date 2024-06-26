using System.Net;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using Redbean.Api;

namespace Redbean;

public class GoogleAuthorization(RequestDelegate next)
{
	public async Task InvokeAsync(HttpContext context)
	{
		if (context.Request.Path.StartsWithSegments("/swagger"))
		{
			var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
			                                                                   {
				                                                                   ClientId = "517818090277-dh7nin47elvha6uhn64ihiboij7pv57p.apps.googleusercontent.com",
				                                                                   ClientSecret = "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"
			                                                                   },
			                                                                   new[] { "openid", "email" },
			                                                                   "user",
			                                                                   CancellationToken.None);

			using var http = new HttpClient
			{
				DefaultRequestHeaders =
				{
					{ "Authorization", "Bearer " + credential.Token.AccessToken }
				}
			};
			var response = await http.GetAsync("https://openidconnect.googleapis.com/v1/userinfo");
			var user = JObject.Parse(await response.Content.ReadAsStringAsync());
			var userEmail = (string)user.GetValue("email");
			
			if (App.AdministratorKey.Contains(userEmail))
			{
				await next.Invoke(context).ConfigureAwait(false);
				return;
			}
			
			context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
		}
		else
			await next.Invoke(context).ConfigureAwait(false);
	}
}