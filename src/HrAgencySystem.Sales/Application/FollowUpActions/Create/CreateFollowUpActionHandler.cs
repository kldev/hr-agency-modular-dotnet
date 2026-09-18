using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.FollowUp;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Sales.Application.FollowUpActions.Create;

public static class CreateFollowUpActionHandler
{
    public static async Task<FollowUpActionCreated> Handle(
        CreateFollowUpAction command,
        ISalesService service,
        CancellationToken ct
    )
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        await service.ValidateOrganization(command.OrganizationId, ct);

        var user = await service.GetUserAsync(command.CreatedBy, ct);

        return await service.AppendFollowUpActionToStream(
            SalesOpportunityId.From(command.OpportunityId),
            organizationId,
            command.Content,
            command.FollowDateTime,
            user,
            ct
        );
    }
}
