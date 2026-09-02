using HrAgencySystem.Identity.Application.Model;
using HrAgencySystem.Identity.Domain;

namespace HrAgencySystem.Identity.Application.Port;

public interface IUserSuggestionRepository
{
    Task<IReadOnlyList<UserSuggestion>>  GetUserSuggestions(Guid organizationId, string search, IReadOnlyList<OrganizationRole> roles, CancellationToken ct);
}