using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Redbean.Api;

namespace Redbean;

public class GoogleAuthorization(RequestDelegate next)
{
	private const int ExpiredSecond = 300;

	public async Task InvokeAsync(HttpContext context)
	{
		if (context.Request.Path.StartsWithSegments("/swagger/index.html"))
		{
			if (context.Request.Query.TryGetValue("state", out var value))
			{
				if (GoogleAuthentication.Tokens.Remove(value, out var user))
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

			var state = $"{Guid.NewGuid()}".Replace("-", "");
			var properties = new AuthenticationProperties
			{
				RedirectUri = $"/swagger/index.html?state={state}"
			};
			await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties);
			
			GoogleAuthentication.Tokens.TryAdd(state, new MiddlewareMetadata
			{
				Expire = DateTime.UtcNow.AddSeconds(ExpiredSecond)
			});
		}
		else
			await next.Invoke(context).ConfigureAwait(false);
	}
}