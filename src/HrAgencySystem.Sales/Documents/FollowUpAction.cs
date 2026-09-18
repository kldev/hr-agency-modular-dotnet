using HrAgencySystem.Sales.Domain.FollowUp;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Documents;

public sealed record FollowUpAction(
    Guid Id,
    Guid OpportunityId,
    Guid OrganizationId,
    string Content,
    DateTimeOffset FollowDateTime,
    DateTimeOffset CreatedAt,
    CompanySnapshot Company,
    UserSnapshot CreatedBy
)
{
    public const string ContentFieldName = "Content";

    public static FollowUpAction Create(
        FollowUpActionId id,
        Guid opportunityId,
        Guid organizationId,
        LongText content,
        DateTimeOffset followDateTime,
        DateTimeOffset createdAt,
        CompanySnapshot company,
        UserSnapshot createdBy
    )
    {
        ArgumentNullException.ThrowIfNull(content);

        return new FollowUpAction(
            id.Value,
            opportunityId,
            organizationId,
            content.Value,
            followDateTime,
            createdAt,
            company,
            createdBy
        );
    }

    public FollowUpAction Update(LongText content, DateTimeOffset followDateTime)
    {
        ArgumentNullException.ThrowIfNull(content);

        return this with
        {
            Content = content.Value,
            FollowDateTime = followDateTime,
        };
    }
}
