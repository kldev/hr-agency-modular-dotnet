namespace HrAgencySystem.Projects.Application.Create;

/// <summary>
/// What creating and updating a project have in common, so one factory validates both.
/// </summary>
public interface IProjectData
{
    string Name { get; }
    string Description { get; }
    string Street { get; }
    string BuildingNumber { get; }
    string? UnitNumber { get; }
    string PostalCode { get; }
    string City { get; }
    string CountryCode { get; }
    DateOnly StartsOn { get; }
    DateOnly? EndsOn { get; }
}
