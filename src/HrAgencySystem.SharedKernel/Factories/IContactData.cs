using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.SharedKernel.Factories;

public interface IContactData
{
    ContactPerson Contact { get; }
}
