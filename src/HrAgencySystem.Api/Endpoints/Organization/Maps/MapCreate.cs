using System.ComponentModel;
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
        group
            .MapPost(ApiEndpoints.Organizations.Create, Handler)
            .WithSummary("Create organization")
            .WithName("Create organization")
            .Produces<OrganizationCreated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        OwnerAuthenticated owner,
        OrganizationRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<OrganizationCreated>(request.ToCommand(owner.Id), ct);

        return TypedResults.Created($"/api/organization/{result.OrganizationId}", result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record OrganizationRequest(
    [property: Description(
        "The agency's display name, shown in the panel and on its public job board."
    )]
        string Name,
    [property: Description(
        "The agency's short address, e.g. \"hr-agency\": part of the job board and feed URLs. Unique across the platform, stored in lower case, up to 100 characters."
    )]
        string Slug,
    [property: Description(
        "The domains the agency's accounts use, e.g. [\"hr-agency.com\"]. Signing in finds the agency by the e-mail domain, so at least one non-empty domain is required."
    )]
        IReadOnlyList<string> EmailDomains,
    [property: Description(
        "Optional contact details of the agency: phone, e-mail, location, website. Omit for none."
    )]
        OrganizationInfoData? Info
)
{
    public CreateOrganization ToCommand(Guid createdBy) =>
        new(Name, Slug, createdBy, EmailDomains, Info);
}
