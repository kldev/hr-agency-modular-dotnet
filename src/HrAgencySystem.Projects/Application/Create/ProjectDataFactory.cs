using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Projects.Application.Create;

internal static class ProjectDataFactory
{
    /// <summary>
    /// Collects every problem into one <see cref="ValidationException"/>, the rhythm the other
    /// factories in this codebase use.
    /// </summary>
    public static (ProjectName name, LongText description, Placement placement) Create(
        IProjectData data
    )
    {
        var errors = new List<string>();

        var (name, nameError) = ProjectName.TryCreate(data.Name);
        if (nameError is not null)
            errors.Add(nameError);

        var (description, descriptionError) = LongText.TryCreate(data.Description);
        if (descriptionError is not null)
            errors.Add(descriptionError);

        var (address, addressErrors) = PostalAddress.TryCreate(
            data.Street,
            data.BuildingNumber,
            data.UnitNumber,
            data.PostalCode,
            data.City,
            data.CountryCode
        );
        errors.AddRange(addressErrors);

        if (data.EndsOn is not null && data.EndsOn < data.StartsOn)
            errors.Add(Placement.EndsBeforeStartMessage);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return (
            name!,
            description!,
            new Placement(address!, address!.CountryCode, data.StartsOn, data.EndsOn)
        );
    }
}
