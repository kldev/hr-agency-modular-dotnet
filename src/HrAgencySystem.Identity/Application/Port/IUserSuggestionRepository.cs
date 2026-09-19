using HrAgencySystem.Identity.Domain;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Application.Port;

public interface IUserSuggestionRepository
{
    Task<IReadOnlyList<UserSuggestion>> GetUserSuggestions(
        OrganizationId organizationId,
        string search,
        IReadOnlyList<OrganizationRole> roles,
        CancellationToken ct
    );

    Task<UserSuggestion?> GetUserSuggestion(
        OrganizationId organizationId,
        Guid userId,
        CancellationToken ct
    );
}

public sealed record UserSuggestion(Guid Id, string FullName, string Email);
