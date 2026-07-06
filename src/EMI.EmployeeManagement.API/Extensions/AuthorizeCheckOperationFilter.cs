using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EMI.EmployeeManagement.API.Extensions
{
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

            var authorizeAttributes = endpointMetadata.OfType<AuthorizeAttribute>().ToList();
            var hasAllowAnonymous = endpointMetadata.OfType<AllowAnonymousAttribute>().Any();

            if (authorizeAttributes.Count == 0 || hasAllowAnonymous)
                return;

            var roles = authorizeAttributes
                .Select(a => a.Roles)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .SelectMany(r => r!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(r => r)
                .ToList();

            operation.Security =
            [
                new OpenApiSecurityRequirement
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
                        roles
                    }
                }
            ];

            var roleText = roles.Count > 0
                ? string.Join(", ", roles)
                : "Any authenticated user";

            var authNote = $"**Authorization:** Bearer JWT required. **Roles:** {roleText}.";

            operation.Description = string.IsNullOrWhiteSpace(operation.Description)
                ? authNote
                : $"{operation.Description}\n\n{authNote}";

            operation.Responses.TryAdd("401", new OpenApiResponse
            {
                Description = "Unauthorized — missing or invalid JWT."
            });

            operation.Responses.TryAdd("403", new OpenApiResponse
            {
                Description = roles.Count > 0
                    ? $"Forbidden — JWT valid but user lacks required role(s): {roleText}."
                    : "Forbidden — JWT valid but user is not allowed to access this resource."
            });
        }
    }
}
