using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.ApiKeys.Issue;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Owner.Maps;

internal static class MapIssueApiKey
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(ApiEndpoints.Owners.ApiKeys, Handler)
            .WithSummary("Issue a key for a program, returning its value once")
            .WithName("Issue service API key")
            .Produces<ServiceApiKeyIssued>(StatusCodes.Status201Created)
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        OwnerAuthenticated owner,
        IssueServiceApiKeyRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var issued = await bus.InvokeAsync<ServiceApiKeyIssued>(
            new IssueServiceApiKey(request.Name, owner.Id),
            ct
        );

        return TypedResults.Created($"{ApiEndpoints.Owners.ApiKeys}/{issued.Id}", issued);
    }

    internal sealed record IssueServiceApiKeyRequest(
        [property: Description(
            "What the key is for, e.g. \"public job board\" - so it can be told apart and revoked later. Up to 100 characters."
        )]
            string Name
    );
}
