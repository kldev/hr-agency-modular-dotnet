namespace HrAgencySystem.Company.Application.Contacts.Create;

public interface IContactData
{
    string Email { get; }
    string FirstName { get; }
    string LastName { get; }
    string JobTitle { get; }
    string Phone { get; }
}