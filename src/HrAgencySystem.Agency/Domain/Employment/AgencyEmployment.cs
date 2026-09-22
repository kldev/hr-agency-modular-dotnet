using HrAgencySystem.Agency.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Agency.Domain.Employment;

/// <summary>
/// What somebody does for us and on what contract. One stream per person, its id derived from the
/// organization and the user, so a second record for the same person has nowhere to live.
/// <para>
/// Here rather than in <c>Identity</c> because it is not a fact about an account: Identity answers
/// "who may sign in and what may they do", this answers "what are they to this company". It is the
/// same question the chart answers from the other side, which is why the two sit together.
/// </para>
/// <para>
/// Not to be confused with <c>Workers.Assignment</c>. That describes somebody we send to a client;
/// this describes somebody who works for us. Two disjoint groups of people, two modules.
/// </para>
/// </summary>
public sealed class AgencyEmployment : IOrganizationDomain
{
    private AgencyEmployment() { }

    public static AgencyEmployment Empty() => new();

    public OrganizationId OrganizationId { get; private set; }
    public Guid UserId { get; private set; }

    public WorkerContractType ContractType { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }
    public decimal? WeeklyHours { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ModifiedAt { get; private set; }
    public Guid? ModifiedById { get; private set; }

    /// <summary>Ended, as opposed to open-ended. Whether it is over today is a question for a date.</summary>
    public bool IsEnded => EndsOn is not null;

    public bool RequiresTimeRecord => TimeRecordPolicy.RequiresTimeRecord(ContractType);

    public void Apply(AgencyEmploymentStarted @event)
    {
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        UserId = @event.UserId;
        ContractType = @event.ContractType;
        StartsOn = @event.StartsOn;
        EndsOn = null;
        WeeklyHours = @event.WeeklyHours;
        CreatedAt = @event.StartedAt;

        Touch(@event.StartedBy, @event.StartedAt);
    }

    public void Apply(AgencyEmploymentTermsChanged @event)
    {
        ContractType = @event.ContractType;
        WeeklyHours = @event.WeeklyHours;

        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(AgencyEmploymentEnded @event)
    {
        EndsOn = @event.EndsOn;

        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    private void Touch(UserSnapshot by, DateTimeOffset at)
    {
        ModifiedAt = at;
        ModifiedById = by.Id;
    }
}

/*
 * No history list is held here. `AgencyEmploymentTermsChanged` carries the day each set of terms
 * took effect, so the stream already is the history - and when leave has to work out an entitlement
 * for a year somebody spent on two contracts, it can read the events or build its own projection.
 * A second copy kept in memory for a module that does not exist yet would be a guess about what it
 * will need.
 */
