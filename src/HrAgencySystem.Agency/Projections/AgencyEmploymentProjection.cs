using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Domain.Employment;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Agency.Projections;

/// <summary>
/// One document per person: what they are to this company. The register the monitoring screen is
/// built on, because it is the only thing that can tell somebody who has not filled their hours in
/// from somebody who never has to.
/// </summary>
public sealed record AgencyEmploymentProjection(
    Guid Id,
    Guid OrganizationId,
    Guid UserId,
    UserSnapshot User,
    WorkerContractType ContractType,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    decimal? WeeklyHours,
    DateTimeOffset CreatedAt,
    UserSnapshot? ModifiedBy,
    DateTimeOffset? ModifiedAt
)
{
    /// <summary>Derived, never stored: it is the law about a contract type, not a fact we record.</summary>
    public bool RequiresTimeRecord => TimeRecordPolicy.RequiresTimeRecord(ContractType);

    public static AgencyEmploymentProjection Create(AgencyEmploymentStarted @event) =>
        new(
            AgencyStreamId.ForEmployment(@event.OrganizationId, @event.UserId),
            @event.OrganizationId,
            @event.UserId,
            @event.User,
            @event.ContractType,
            @event.StartsOn,
            null,
            @event.WeeklyHours,
            @event.StartedAt,
            @event.StartedBy,
            @event.StartedAt
        );

    public AgencyEmploymentProjection Apply(AgencyEmploymentTermsChanged @event) =>
        this with
        {
            ContractType = @event.ContractType,
            WeeklyHours = @event.WeeklyHours,
            ModifiedBy = @event.ModifiedBy,
            ModifiedAt = @event.ModifiedAt,
        };

    public AgencyEmploymentProjection Apply(AgencyEmploymentEnded @event) =>
        this with
        {
            EndsOn = @event.EndsOn,
            ModifiedBy = @event.ModifiedBy,
            ModifiedAt = @event.ModifiedAt,
        };

    /// <summary>Whether the engagement runs at any point inside the given month.</summary>
    public bool CoversMonth(int year, int month)
    {
        var first = new DateOnly(year, month, 1);
        var last = first.AddMonths(1).AddDays(-1);

        return StartsOn <= last && (EndsOn is null || EndsOn >= first);
    }
}
