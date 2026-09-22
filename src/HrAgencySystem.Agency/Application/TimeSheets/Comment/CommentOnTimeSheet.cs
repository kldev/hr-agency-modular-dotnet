using HrAgencySystem.Agency.Domain;

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
    public Guid Id => AgencyStreamId.ForTimeSheet(OrganizationId, UserId, Year, Month);
}
