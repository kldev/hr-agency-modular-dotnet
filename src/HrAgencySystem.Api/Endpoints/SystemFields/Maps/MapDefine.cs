using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.SystemFields.Define;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.SystemFields.Maps;

internal static class MapDefine
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.SystemFields.Define, Handler)
            .RequireAuthorization(FormsDesignPolicy.Name)
            .WithSummary("Define a system field")
            .WithName("Define system field")
            .ProducesStandardErrors()
            .Produces<SystemFieldDefined>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        DefineSystemFieldRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<SystemFieldDefined>(
                request.ToCommand(user.GetOrganization, user.UserId),
                ct
            )
        );

    internal sealed record DefineSystemFieldRequest(
        [property: Description("employee.<name>, e.g. employee.pesel. Never changes.")] string Code,
        [property: Description("Never changes - a different type is a different field.")] FieldType Type,
        string Label,
        string? Description,
        FieldRules? Rules,
        IReadOnlyList<ChoiceOption>? Options,
        [property: Description("Where a new response may be pre-filled from. None for most fields.")]
            SystemFieldSource Source
    )
    {
        public DefineSystemField ToCommand(OrganizationId organizationId, Guid createdBy) =>
            new(organizationId.Value, Code, Type, Label, Description, Rules, Options, Source, createdBy);
    }
}
