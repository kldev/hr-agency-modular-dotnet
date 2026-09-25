using System.Diagnostics;
using HrAgencySystem.Identity.Infrastructure.IAM;
using Serilog;
using Serilog.Context;

namespace HrAgencySystem.Api.Infrastructure;

/// <summary>
/// Puts "which organization, which user" on everything a request produces: every log line written
/// while it runs, its one-line request summary and its span. In a multi-tenant system that is the
/// first question about any failure, and the token already answers it.
/// Identifiers only - the e-mail and the name in the token stay out, like everywhere else.
/// </summary>
public sealed class TenantTelemetryMiddleware(RequestDelegate next)
{
    public const string OrganizationIdTag = "hr.organization_id";
    public const string UserIdTag = "user.id";
    public const string ImpersonatedByTag = "hr.impersonated_by";

    public async Task InvokeAsync(HttpContext context, IDiagnosticContext diagnostics)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var organizationId = user.FindFirst(AppClaims.OrganizationId)?.Value;
        var userId = user.FindFirst(AppClaims.UserId)?.Value;
        var impersonatedBy = user.FindFirst(AppClaims.ImpersonatedBy)?.Value;

        var activity = Activity.Current;
        activity?.SetTag(OrganizationIdTag, organizationId);
        activity?.SetTag(UserIdTag, userId);
        activity?.SetTag(ImpersonatedByTag, impersonatedBy);

        // The request summary is written by the outer request logging middleware after this scope
        // has closed, so it gets the values through the diagnostic context instead.
        diagnostics.Set("OrganizationId", organizationId);
        diagnostics.Set("UserId", userId);
        if (impersonatedBy is not null)
            diagnostics.Set("ImpersonatedBy", impersonatedBy);

        using (LogContext.PushProperty("OrganizationId", organizationId))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("ImpersonatedBy", impersonatedBy))
        {
            await next(context);
        }
    }
}

public static class TenantTelemetryMiddlewareExtensions
{
    extension(IApplicationBuilder app)
    {
        /// <summary>Belongs right after authentication - before it there is no user to read.</summary>
        public IApplicationBuilder UseTenantTelemetry() =>
            app.UseMiddleware<TenantTelemetryMiddleware>();
    }
}
