using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Documents;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.CompanyContacts.Maps;

internal static class MapGetAll
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{companyId:guid}", Handler)
            .WithSummary("Get contacts")
            .Produces<IReadOnlyList<CompanyContact>>()
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        ICompanyContactQueryRepository repository,
        Guid companyId,
        CancellationToken ct)
    {
        var result = 
            await repository.GetAllAsync(user.OrganizationId, companyId, ct);
        return TypedResults.Ok(result);
    }
}