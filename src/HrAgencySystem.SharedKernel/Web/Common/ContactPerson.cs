using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.SharedKernel.Web.Common;

public sealed record ContactPerson(
    string Email,
    string FirstName,
    string LastName,
    string JobTitle,
    string Phone
)
{
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
    public string Fullname { get; } = $"{FirstName} {LastName}".Trim();

    public ContactPerson ToContact() =>
        new(Email.Value, FirstName.Value, LastName.Value, JobTitle.Value, Phone.Value);
};
