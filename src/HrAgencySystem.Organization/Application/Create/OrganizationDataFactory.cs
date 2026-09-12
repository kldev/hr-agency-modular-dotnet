
using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Organization.Application.Create;

internal static class OrganizationDataFactory
{
    internal static OrganizationData Create(IOrganizationData command)
    {
        var errors = new List<string>();
        var (slug, errorSlug) = OrganizationSlug.TryCreate(command.Slug);
        var (name, errorName) = OrganizationName.TryCreate(command.Name);

        if (errorSlug != null) errors.Add(errorSlug);
        if (errorName != null) errors.Add(errorName);

        
        return errors.Count > 0 ? throw new ValidationException(errors) 
            : new OrganizationData(name!, slug!);
    }

    internal sealed record OrganizationData(
        OrganizationName Name,
        OrganizationSlug Slug);
}