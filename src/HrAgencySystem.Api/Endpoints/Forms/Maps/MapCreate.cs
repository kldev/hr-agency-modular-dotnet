using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.FormDefinitions.Create;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Forms.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.Forms.Create, Handler)
            .RequireAuthorization(FormsDesignPolicy.Name)
            .WithSummary("Create a form")
            .WithName("Create form")
            .ProducesStandardErrors()
            .Produces<FormDefinitionCreated>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        CreateFormRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<FormDefinitionCreated>(
                request.ToCommand(user.GetOrganization, user.UserId),
                ct
            )
        );

    internal sealed record CreateFormRequest(
        [property: Description("Stable code, unique in the organization, e.g. gdpr-consent. Never changes.")]
            string Code,
        [property: Description("What people read.")] string Name,
        string? Description,
        [property: Description("Document or Survey - a label and a default, not a different mechanism.")]
            FormKind Kind,
        [property: Description("One response per person or many. Left out: one for a document, many for a survey. Never changes.")]
            ResponseCardinality? Cardinality,
        [property: Description("Whose responses these are. Only 'worker' today.")] string? SubjectKind
    )
    {
        public CreateFormDefinition ToCommand(OrganizationId organizationId, Guid createdBy) =>
            new(
                organizationId.Value,
                Code,
                Name,
                Description,
                Kind,
                Cardinality,
                SubjectKind ?? SubjectKinds.Worker,
                createdBy
            );
    }
}
