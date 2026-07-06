using Microsoft.OpenApi.Models;

namespace EMI.EmployeeManagement.API.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "EMI Employee Management API",
                    Version = "v1",
                    Description =
                        "JWT-protected API. Call **POST /api/Auth/login** to obtain a token, " +
                        "then click **Authorize** and enter the token value (without the `Bearer` prefix). " +
                        "Swagger will send it as `Authorization: Bearer <token>` on protected endpoints."
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT obtained from POST /api/Auth/login. Example: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`"
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            return services;
        }

        public static WebApplication UseSwaggerDocumentation(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "EMI Employee Management API v1");
                options.RoutePrefix = "swagger";
                options.DisplayRequestDuration();
                options.EnablePersistAuthorization();
            });

            return app;
        }
    }
}
