using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.FollowUp;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Projections;

public sealed record OpportunityProjection(
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid Id,
    Guid OrganizationId,
    Guid CompanyId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    CompanySnapshot Company,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string Title,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string Description,
    OpportunityStage Stage,
    bool IsHotLead,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    decimal ExpectedValue,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    CurrencyCode CurrencyCode,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    DateOnly? ExpectedCloseDate,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string LostReason,
    Guid ResponsibleId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    UserSnapshot Responsible,
    DateTimeOffset CreatedAt,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    UserSnapshot CreatedBy,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    UserSnapshot? ModifiedBy,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    DateTimeOffset? ModifiedAt,
    // the latest follow up action of the opportunity, kept in sync from the follow up events
    Guid? FollowUpActionId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string? FollowUpContent,
    DateTimeOffset? FollowUpDateTime,
    // when the deal was last worked on, from the activity copies on the opportunity stream
    DateTimeOffset? LastActivityAt = null,
    SalesActivityType? LastActivityType = null
) : IAudit
{
    public static OpportunityProjection Create(OpportunityCreated @event)
    {
        return new OpportunityProjection(
            @event.OpportunityId,
            @event.OrganizationId,
            @event.Company.Id,
            @event.Company,
            @event.Title,
            @event.Description,
            @event.Stage,
            @event.IsHotLead,
            @event.ExpectedValue,
            @event.Currency,
            @event.ExpectedCloseDate,
            "",
            @event.Responsible.Id,
            @event.Responsible,
            @event.CreatedAt,
            @event.CreatedBy,
            null,
            null,
            null,
            null,
            null
        );
    }

    public OpportunityProjection Apply(OpportunityUpdated @event)
    {
        return this with
        {
            Title = @event.Title,
            Description = @event.Description,
            ExpectedValue = @event.ExpectedValue,
            ExpectedCloseDate = @event.ExpectedCloseDate,
            CurrencyCode = @event.Currency,
            IsHotLead = @event.IsHotLead,
        };
    }

    public OpportunityProjection Apply(StageChanged @event)
    {
        return this with
        {
            Stage = @event.Stage,
            LostReason = @event.LostReason,
            ModifiedAt = @event.ChangedAt,
            ModifiedBy = @event.ChangedBy,
        };
    }

    public OpportunityProjection Apply(ResponsiblePersonChanged @event)
    {
        return this with
        {
            Responsible = @event.Responsible,
            ModifiedAt = @event.ChangedAt,
            ModifiedBy = @event.ChangedBy,
        };
    }

    // an activity backdated behind the latest one does not move the date back
    public OpportunityProjection Apply(OpportunityActivityLogged @event)
    {
        return LastActivityAt is { } last && last > @event.LoggedAt
            ? this
            : this with { LastActivityAt = @event.LoggedAt, LastActivityType = @event.ActivityType };
    }

    public OpportunityProjection Apply(FollowUpActionCreated @event)
    {
        return TrackFollowUpAction(@event.FollowUpActionId, @event.Content, @event.FollowDateTime);
    }

    public OpportunityProjection Apply(FollowUpActionUpdated @event)
    {
        return TrackFollowUpAction(@event.FollowUpActionId, @event.Content, @event.FollowDateTime);
    }

    private OpportunityProjection TrackFollowUpAction(
        Guid followUpActionId,
        string content,
        DateTimeOffset followDateTime
    )
    {
        return IsLatestFollowUpAction(followUpActionId, followDateTime)
            ? this with
            {
                FollowUpActionId = followUpActionId,
                FollowUpContent = content,
                FollowUpDateTime = followDateTime,
            }
            : this;
    }

    // the salesperson only edits the current entry, so the entry with the latest
    // follow up date wins - an edit of the tracked entry always wins
    private bool IsLatestFollowUpAction(Guid followUpActionId, DateTimeOffset followDateTime)
    {
        if (FollowUpActionId is null)
            return true;

        return FollowUpActionId == followUpActionId
            || followDateTime >= FollowUpDateTime.GetValueOrDefault();
    }
}
