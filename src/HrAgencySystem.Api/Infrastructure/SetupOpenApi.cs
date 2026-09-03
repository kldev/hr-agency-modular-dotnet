using Microsoft.OpenApi;

namespace HrAgencySystem.Api.Infrastructure;

internal static class SetupOpenApi
{
    internal static void AddAppOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "HR Agency System";
                document.Info.Version = "v1";

                document.Components ??= new OpenApiComponents();

                document.Components.SecuritySchemes ??=
                    new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes["bearer"] =
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter JWT Bearer token"
                    };

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, _) =>
            {
                var allowAnonymous = context.Description.ActionDescriptor.EndpointMetadata
                    .OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>()
                    .Any();

                if (!allowAnonymous)
                {
                    operation.Security ??= [];

                    operation.Security.Add(
                        new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecuritySchemeReference("bearer")] = []
                        });
                }

                return Task.CompletedTask;
            });
        });
    }
}