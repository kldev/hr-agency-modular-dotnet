using HrAgencySystem.Sales.Domain.FollowUp;
using HrAgencySystem.Sales.Events.FollowUp;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Sales.Application.FollowUpActions.Update;

public static class UpdateFollowUpActionHandler
{
    public static async Task<FollowUpActionUpdated> Handle(
        UpdateFollowUpAction command,
        ISalesService service,
        CancellationToken ct)
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        await service.ValidateOrganization(command.OrganizationId, ct);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        return await service.AppendFollowUpActionUpdateToStream(
            FollowUpActionId.From(command.FollowUpActionId),
            organizationId,
            command.Content,
            command.FollowDateTime,
            user,
            ct);
    }
}
