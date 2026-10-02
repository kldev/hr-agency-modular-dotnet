using HrAgencySystem.SharedKernel.ValueObjects;
using JetBrains.Annotations;

namespace HrAgencySystem.SharedKernel.Web.Common;

public sealed record ContactPerson(
    string Email,
    string FirstName,
    string LastName,
    string JobTitle,
    string Phone
)
{
    [UsedImplicitly] // Serialized into the API response; the front end reads it.
    public string Fullname { get; } = $"{FirstName} {LastName}".Trim();
}

public sealed record ContactPersonValueObject(
    Email Email,
    FirstName FirstName,
    LastName LastName,
    PersonJobTitle JobTitle,
    PersonPhone Phone
)
{
    public ContactPerson ToContact() =>
        new(Email.Value, FirstName.Value, LastName.Value, JobTitle.Value, Phone.Value);
};
