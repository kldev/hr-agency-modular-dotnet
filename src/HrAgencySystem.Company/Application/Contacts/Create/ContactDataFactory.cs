using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Company.Application.Contacts.Create;

internal static class ContactDataFactory
{
    internal static ContactData Create(IContactData data)
    {
        var errors = new List<string>();

        var (email, emailError) = Email.TryCreate(data.Email);
        var (firstName, firstNameError) = FirstName.TryCreate(data.FirstName);
        var (lastName, lastNameError) = LastName.TryCreate(data.LastName);
        var (jobTitle, jobTitleError) = PersonJobTitle.TryCreate(data.JobTitle);

        if (emailError != null) errors.Add(emailError);
        if (firstNameError != null) errors.Add(firstNameError);
        if (lastNameError != null) errors.Add(lastNameError);
        if (jobTitleError != null) errors.Add(jobTitleError);
        
        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new ContactData(
            email!, firstName!, lastName!, jobTitle!);
    }
    internal sealed record ContactData(Email Email, 
        FirstName FirstName, 
        LastName LastName,
        PersonJobTitle JobTitle);
}