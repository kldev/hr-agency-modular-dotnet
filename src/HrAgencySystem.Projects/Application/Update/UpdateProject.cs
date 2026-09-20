using HrAgencySystem.Projects.Application.Create;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Projects.Application.Update;

public sealed record UpdateProject(
    [property: Identity] Guid ProjectId,
    Guid OrganizationId,
    string Name,
    string Description,
    string Street,
    string BuildingNumber,
    string? UnitNumber,
    string PostalCode,
    string City,
    string CountryCode,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    Guid ModifiedBy
) : IUpdateCommand, IProjectData;
