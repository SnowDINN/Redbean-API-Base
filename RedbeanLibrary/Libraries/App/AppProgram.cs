using System.Security.Claims;
using System.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Redbean.Swagger;

namespace Redbean;

public class AppProgram
{
	public static Task Build(WebApplicationBuilder builder)
	{
		builder.Services.AddAuthorization();
		builder.Services.AddAuthentication(options =>
			{
				options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme =
					$"{GoogleDefaults.AuthenticationScheme},{JwtBearerDefaults.AuthenticationScheme}";
				options.DefaultAuthenticateScheme =
					$"{GoogleDefaults.AuthenticationScheme},{JwtBearerDefaults.AuthenticationScheme}";
			})
			.AddCookie()
			.AddGoogle(options =>
			{
				options.ClientId = AppEnvironment.Default.Swagger.OauthClientId;
				options.ClientSecret = AppEnvironment.Default.Swagger.OauthClientSecretId;
				options.SaveTokens = true;
				options.Events.OnCreatingTicket = ticket =>
				{
					var queryCollection = ticket.Properties.RedirectUri.Split('?').Last();
					var query = HttpUtility.ParseQueryString(queryCollection);
					var email = ticket.Identity.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Email).Value;
					AppToken.SwaggerSessionTokens[query["session"]].isAuthentication =
						JwtPermission.IsAdministratorExist(email);

					return Task.CompletedTask;
				};
			})
			.AddJwtBearer(options =>
			{
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					// 토큰 만료시간 오차
					ClockSkew = TimeSpan.FromSeconds(120),
					IssuerSigningKey = JwtSecurity.SecurityKey,
					ValidateAudience = false,
					ValidateIssuer = false,
					ValidateIssuerSigningKey = true
				};
			});
		builder.Services.AddControllersWithViews().AddNewtonsoftJson();
		builder.Services.AddSwaggerGenNewtonsoftSupport();
		builder.Services.AddSwaggerGen(options =>
		{
			// JWT 인증
			options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Name = "Authorization",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.Http,
				Scheme = "Bearer"
			});
			
			// 클라이언트 버전 인증
			options.AddSecurityDefinition("Version", new OpenApiSecurityScheme
			{
				Name = "Version",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.ApiKey
			});
			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					Array.Empty<string>()
				},
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Version"
						}
					},
					Array.Empty<string>()
				}
			});
		});
		
		return Task.CompletedTask;
	}

	public static async Task Run(WebApplicationBuilder builder)
	{
		var app = builder.Build();
		app.UseAuthorization();
		app.UseAuthentication();
		app.UseHttpsRedirection();
		app.MapControllers();

		if (app.Environment.IsDevelopment())
		{
			// Swagger Authorization
			app.UseMiddleware<SwaggerGoogleAuthentication>();
	
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		await app.RunAsync();
	}
}