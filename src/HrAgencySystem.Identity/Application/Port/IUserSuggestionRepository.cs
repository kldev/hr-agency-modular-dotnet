using HrAgencySystem.Identity.Domain;

namespace HrAgencySystem.Identity.Application.Port;

public interface IUserSuggestionRepository
{
    Task<IReadOnlyList<UserSuggestion>>  GetUserSuggestions(Guid organizationId, string search, IReadOnlyList<OrganizationRole> roles, CancellationToken ct);
}

public sealed record UserSuggestion(Guid Id, string FullName, string Email);