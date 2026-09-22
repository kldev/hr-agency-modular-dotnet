using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.SharedKernel.Services;

namespace HrAgencySystem.Api.Endpoints.Internal.Maps;

internal static class MapGetBoard
{
    internal static void Map(RouteGroupBuilder group) =>
        group.MapGet(ApiEndpoints.Internal.Board, Handler).WithSummary("An agency's board by slug");

    private static async Task<IResult> Handler(
        IQueryOrganizationRepository organizations,
        string slug,
        CancellationToken ct
    )
    {
        var organization = await organizations.GetBySlugAsync(slug, ct);

        return organization is null
            ? TypedResults.NotFound(DomainObjectNotFound.NotFound("Board", slug))
            : TypedResults.Ok(new BoardResponse(organization.Slug, organization.Name));
    }

    internal sealed record BoardResponse(string Slug, string Name);
}
