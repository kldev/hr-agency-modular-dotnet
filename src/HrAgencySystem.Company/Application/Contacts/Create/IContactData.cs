using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Company.Application.Contacts.Create;

public interface IContactData
{
    ContactPerson Contact
    {
        get;
    }
}