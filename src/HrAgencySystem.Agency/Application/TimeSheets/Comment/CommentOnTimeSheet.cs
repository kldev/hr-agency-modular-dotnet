using HrAgencySystem.Agency.Domain;
using JetBrains.Annotations;

namespace HrAgencySystem.Agency.Application.TimeSheets.Comment;

public sealed record CommentOnTimeSheet(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    string Content,
    bool ActingAsPayroll,
    Guid AuthorId
)
{
    [UsedImplicitly] // Wolverine's [AggregateHandler] loads the stream by this id.
    public Guid Id => AgencyStreamId.ForTimeSheet(OrganizationId, UserId, Year, Month);
}
