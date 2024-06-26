using System.Net;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using Redbean.Api;

namespace Redbean.Swagger;

public class SwaggerGoogleAuthorization(RequestDelegate next)
{
	public async Task InvokeAsync(HttpContext context)
	{
		if (context.Request.Path.StartsWithSegments("/swagger"))
		{
			var token = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
			                                                             {
				                                                             ClientId = "517818090277-dh7nin47elvha6uhn64ihiboij7pv57p.apps.googleusercontent.com",
				                                                             ClientSecret = "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"
			                                                             },
			                                                             Array.Empty<string>(),
			                                                             "user",
			                                                             CancellationToken.None);

			using var http = new HttpClient
			{
				DefaultRequestHeaders =
				{
					{ "Authorization", "Bearer " + token.Token.AccessToken }
				}
			};
			
			var request = await http.GetAsync("https://openidconnect.googleapis.com/v1/userinfo");
			var userInfo = JObject.Parse(await request.Content.ReadAsStringAsync());
			if (App.AdministratorKey.Contains((string)userInfo.GetValue("email")))
			{
				await next.Invoke(context).ConfigureAwait(false);
				return;
			}

			context.Response.Headers.WWWAuthenticate = "Basic";
			context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
		}
		else
			await next.Invoke(context).ConfigureAwait(false);
	}
}