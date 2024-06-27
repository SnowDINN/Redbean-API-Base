using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Redbean.Api;

public sealed class AuthorizationOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
		operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

		operation.Security = new List<OpenApiSecurityRequirement>
		{
			new()
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = GoogleAuthentication.GoogleScheme
						}
					},
					new List<string>()
				}
			}
		};
	}
}