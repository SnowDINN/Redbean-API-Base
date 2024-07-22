using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

namespace Redbean.Swagger;

public class SwaggerGoogleAuthentication(RequestDelegate next)
{
	private const int ExpiredSecond = 300;

	public async Task InvokeAsync(HttpContext context)
	{
		if (context.Request.Path.StartsWithSegments("/swagger/index.html"))
		{
			if (context.Request.Query.TryGetValue("session", out var value))
			{
				if (AppToken.SwaggerSessionTokens.Remove(value, out var user))
				{
					if (user.isAuthentication)
					{
						await next.Invoke(context).ConfigureAwait(false);
						return;
					}
					
					context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
					return;
				}
			}

			var session = $"{Guid.NewGuid()}".Replace("-", "");
			var properties = new AuthenticationProperties
			{
				RedirectUri = $"/swagger/index.html?session={session}"
			};
			await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties);
			
			AppToken.SwaggerSessionTokens.TryAdd(session, new SwaggerSessionToken
			{
				Expire = DateTime.UtcNow.AddSeconds(ExpiredSecond)
			});
		}
		else
			await next.Invoke(context).ConfigureAwait(false);
	}
}