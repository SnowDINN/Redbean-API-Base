using System.Security.Claims;
using System.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Redbean;
using Redbean.Api;

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
		options.ClientId = "517818090277-b4n17aclsf2ie6e2c06e6fqbqhh9d03u.apps.googleusercontent.com";
		options.ClientSecret = "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";
		
		options.SaveTokens = true;
		options.Events.OnCreatingTicket = ticket =>
		{
			var queryCollection = ticket.Properties.RedirectUri.Split('?').Last();
			var query = HttpUtility.ParseQueryString(queryCollection);
			
			var email = ticket.Identity.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Email).Value;
			GoogleAuthentication.Tokens[query["state"]].isAuthentication = Authorization.Administrators.Contains(email);

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
	options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
	{
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.Http,
		Scheme = JwtBearerDefaults.AuthenticationScheme
	});
	
	options.AddSecurityDefinition(AppDefaults.VersionScheme, new OpenApiSecurityScheme
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
					Id = JwtBearerDefaults.AuthenticationScheme
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
					Id = AppDefaults.VersionScheme
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
	app.UseMiddleware<GoogleAuthorization>();
	
	app.UseSwagger();
	app.UseSwaggerUI();
}

await app.RunAsync();