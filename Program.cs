using System.Security.Claims;
using System.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Redbean;
using Redbean.Middleware;
using Redbean.Security;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
	{
		options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = $"{GoogleDefaults.AuthenticationScheme},{JwtBearerDefaults.AuthenticationScheme}";
		options.DefaultAuthenticateScheme = $"{GoogleDefaults.AuthenticationScheme},{JwtBearerDefaults.AuthenticationScheme}";
	})
	.AddCookie()
	.AddGoogle(options =>
	{
		options.ClientId = EnvironmentSettings.Default.Swagger.OauthClientId;
		options.ClientSecret = EnvironmentSettings.Default.Swagger.OauthClientSecretId;
		
		options.SaveTokens = true;
		options.Events.OnCreatingTicket = ticket =>
		{
			var queryCollection = ticket.Properties.RedirectUri.Split('?').Last();
			var query = HttpUtility.ParseQueryString(queryCollection);
			
			var email = ticket.Identity.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Email).Value;
			GoogleAuthentication.Tokens[query["session"]].isAuthentication = SecurityRole.AdministratorEmails.Contains(email);

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
			
			IssuerSigningKey = new SymmetricSecurityKey(AppSecurity.SecurityKey),
			ValidateAudience = false,
			ValidateIssuer = false,
			ValidateIssuerSigningKey = true
		};
	});
builder.Services.AddControllersWithViews().AddNewtonsoftJson();
builder.Services.AddSwaggerGenNewtonsoftSupport();
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.Http,
		Scheme = "Bearer"
	});
	
	options.AddSecurityDefinition("Version", new OpenApiSecurityScheme
	{
		Name = "Version",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
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

await Bootstrap.Setup();

var app = builder.Build();
app.UseAuthorization();
app.UseAuthentication();
app.UseHttpsRedirection();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
	// Swagger Authorization
	app.UseMiddleware<GoogleAuthenticationMiddleware>();
	
	app.UseSwagger();
	app.UseSwaggerUI();
}

await app.RunAsync();