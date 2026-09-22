using HrAgencySystem.Api.Auth;

namespace HrAgencySystem.Api.Endpoints.Internal;

/// <summary>
/// What the public job board reads and writes, and nothing else. Behind a service key only - the
/// policy names the key scheme alone, so a user's token does not open these routes - and left out
/// of the OpenAPI document: this is a contract between two of our own processes, not an API for
/// anybody to discover.
/// <para>
/// The responses are records of their own rather than read models. The board is deployed apart
/// from the API, so a field added to a projection for the agency panel must not change what the
/// board receives.
/// </para>
/// </summary>
internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("")
            .WithTags("Internal")
            .RequireAuthorization(InternalApiPolicy.Name)
            .ExcludeFromDescription();

        Maps.MapGetBoard.Map(group);
        Maps.MapGetFeed.Map(group);
        Maps.MapGetPost.Map(group);
        Maps.MapApply.Map(group);
    }
}
