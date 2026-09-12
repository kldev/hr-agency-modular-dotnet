namespace HrAgencySystem.SharedKernel.Web.Common;

public sealed record ContactPerson(
    string Email,
    string FirstName,
    string LastName,
    string JobTitle,
    string Phone)
{
    public string Fullname { get; } = $"{FirstName} {LastName}".Trim();
};