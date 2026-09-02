using HrAgencySystem.Identity.Domain;

namespace HrAgencySystem.Identity.Application.Commands;

public sealed record CreateUser(
    Guid OrganizationId,
    string Email,
    string FirstName,
    string LastName,
    OrganizationRole Role,
    string Password,
    Guid CreatedBy);