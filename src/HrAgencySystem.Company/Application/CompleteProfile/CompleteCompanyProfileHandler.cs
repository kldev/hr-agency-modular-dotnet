using HrAgencySystem.Company.Events;
using HrAgencySystem.Company.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Company.Application.CompleteProfile;

public static class CompleteCompanyProfileHandler
{
    [AggregateHandler]
    public static async Task<(CompanyProfileUpdated, Wolverine.Marten.Events)> Handle(
        CompleteCompanyProfile command,
        Domain.Company aggregate,
        ICompanyService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        if (aggregate.OrganizationId.Value != command.OrganizationId)
            throw new OrganizationAccessDeniedException();

        var user = await service.GetUserAsync(command.ModifiedBy, ct);
        var profile = CompanyProfileFactory.Create(command);

        var @event = new CompanyProfileUpdated(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            profile,
            user,
            clock.UtcNow
        );

        // The crossing is the news, not the saving. Raised only when the profile was not complete
        // before and is now, so "when did this company become contractable" has one answer.
        var wasComplete = aggregate.IsProfileComplete;
        var isComplete = profile.IsComplete && aggregate.TaxId is not null;

        if (wasComplete || !isComplete)
            return (@event, [@event]);

        return (
            @event,
            [
                @event,
                new CompanyProfileCompleted(
                    aggregate.Id.Value,
                    aggregate.OrganizationId.Value,
                    user,
                    clock.UtcNow
                ),
            ]
        );
    }
}
