using HrAgencySystem.Compliance;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Projects.Application.Create;

public sealed record CreateProject(
    Guid OrganizationId,
    Guid CompanyId,
    Guid LegalEntityId,
    string Name,
    string Description,
    EngagementType EngagementType,
    string Street,
    string BuildingNumber,
    string? UnitNumber,
    string PostalCode,
    string City,
    string CountryCode,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    Guid? TeamId,
    Guid CreatedBy
) : ICreateCommand, IProjectData;
