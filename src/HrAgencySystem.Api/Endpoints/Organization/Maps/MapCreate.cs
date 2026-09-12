using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Organization.Application.Create;
using HrAgencySystem.Organization.Events;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

internal static class MapCreate
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("", Handler)
            .WithSummary("Create organization")
            .WithName("Create organization")
            .Produces<OrganizationCreated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(OwnerAuthenticated owner, CreateOrganizationRequest request,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<OrganizationCreated>(request.ToCommand(owner.Id), ct);

        return TypedResults.Created(
            $"/api/organization/{result.OrganizationId}", result);
    }
}

internal sealed record CreateOrganizationRequest(string Name, string Slug, IReadOnlyList<string> EmailDomains)
{
    public CreateOrganization ToCommand(Guid createdBy) => new (Name, Slug, createdBy, EmailDomains);
}