using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Company.Application.Contacts.Create;

internal static class ContactDataFactory
{
    internal static ContactPerson Create(IContactData data)
    {
        var errors = new List<string>();

        var (email, emailError) = Email.TryCreate(data.Contact.Email);
        var (firstName, firstNameError) = FirstName.TryCreate(data.Contact.FirstName);
        var (lastName, lastNameError) = LastName.TryCreate(data.Contact.LastName);
        var (jobTitle, jobTitleError) = PersonJobTitle.TryCreate(data.Contact.JobTitle);
        var (phone, phoneError) = PersonPhone.TryCreate(data.Contact.Phone);

        if (emailError != null) errors.Add(emailError);
        if (firstNameError != null) errors.Add(firstNameError);
        if (lastNameError != null) errors.Add(lastNameError);
        if (jobTitleError != null) errors.Add(jobTitleError);
        if (phoneError != null) errors.Add(phoneError);
        
        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new ContactPerson(
            email!.Value, 
            firstName!.Value, 
            lastName!.Value, 
            jobTitle!.Value, 
            phone!.Value);
    }
    
}