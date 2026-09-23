using HrAgencySystem.Api.Infrastructure.OpenApi;
using HrAgencySystem.ReportsService.Contracts;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.Api.Endpoints.Reports;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        // An organization's own numbers: any signed-in member, no role - they are its own.
        var organization = endpoints.MapGroup("").WithTags(ApiTags.Reports);

        Maps.MapGetRecruitment.Map(organization);
        Maps.MapExportRecruitment.Map(organization);

        var owner = endpoints.MapGroup("").WithTags(ApiTags.Reports).WithOwnerRole();

        Maps.MapGetPlatform.Map(owner);
        Maps.MapExportPlatform.Map(owner);
    }

    /// <summary>
    /// The period from the query string, checked here so a person gets a 400 with the reason
    /// instead of the service's refusal surfacing as a 503.
    /// </summary>
    internal static ReportPeriod Period(string? from, string? to, IClock clock)
    {
        var (period, error) = ReportPeriod.TryCreate(
            from,
            to,
            DateOnly.FromDateTime(clock.UtcNow.UtcDateTime)
        );

        return period ?? throw new ValidationException(error!);
    }
}
