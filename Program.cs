using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Redbean;
using Redbean.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
		BearerFormat = "JWT",
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