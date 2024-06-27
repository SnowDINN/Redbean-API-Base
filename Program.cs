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
builder.Services.AddAuthentication(options =>
	{
		options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddCookie()
	.AddGoogle(options =>
	{
		options.ClientId = "517818090277-b4n17aclsf2ie6e2c06e6fqbqhh9d03u.apps.googleusercontent.com";
		options.ClientSecret = "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";
		
		options.Events.OnCreatingTicket = ticket =>
		{
			var queryCollection = ticket.Properties.RedirectUri.Split('?').Last();
			var query = HttpUtility.ParseQueryString(queryCollection);
			
			var email = ticket.Identity.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Email).Value;
			App.State[query["state"]].isAuthentication = App.AdministratorKey.Contains(email);

			return Task.CompletedTask;
		};
	})
	.AddJwtBearer(options =>
	{
		options.SaveToken = true;
		options.RequireHttpsMetadata = true;
		options.TokenValidationParameters = new TokenValidationParameters
		{
			// 토큰 만료시간 오차
			ClockSkew = TimeSpan.FromSeconds(120),
			
			IssuerSigningKey = new SymmetricSecurityKey(App.SecurityKey),
			ValidateAudience = false,
			ValidateIssuer = false,
			ValidateIssuerSigningKey = true
		};
	});
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
	{
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = JwtBearerDefaults.AuthenticationScheme
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