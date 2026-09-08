namespace HrAgencySystem.Company.Documents;

public sealed record CompanyContact(
    Guid Id, 
    Guid OrganizationId,
    Guid CompanyId, 
    string Email, 
    string FirstName, 
    string LastName, 
    string JobTitle,
    string Phone,
    string CompanyName,
    DateTimeOffset CreatedAt);