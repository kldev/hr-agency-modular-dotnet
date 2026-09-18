using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Projections;

public sealed record UserProjection(
    Guid Id,
    Guid OrganizationId,
    string Email,
    string FirstName,
    string LastName,
    OrganizationRole Role,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid CreatedById,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt,
    OrganizationInfo Organization,
    string Phone = "",
    UserSnapshot? ModifiedBy = null,
    DateTimeOffset? ModifiedAt = null
) : IAudit
{
    public static UserProjection Create(UserCreated @event)
    {
        return new UserProjection(
            @event.UserId,
            @event.OrganizationId,
            @event.Email,
            @event.FirstName,
            @event.LastName,
            @event.Role,
            @event.CreatedBy.Id,
            @event.CreatedBy,
            @event.CreatedAt,
            @event.Organization,
            @event.Phone ?? "",
            null,
            null
        );
    }

    public string FullName => $"{FirstName} {LastName}";
}
