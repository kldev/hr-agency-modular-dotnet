namespace HrAgencySystem.EmailTemplates.Contracts.Teams;

/// <summary>
/// Sent when somebody puts another person on a team; adding yourself is not news to anybody.
/// Role travels as a string because this project has no dependencies and a team role enum would
/// be one.
/// </summary>
public sealed record SendTeamMemberAdded(
    Guid EventId,
    string Source,
    Guid TeamId,
    string TeamName,
    string MemberEmail,
    string MemberFullname,
    string Role,
    string AddedByFullname
) : IEmailTemplateContract;
