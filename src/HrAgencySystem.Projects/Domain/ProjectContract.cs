using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// The contract behind the project. Not an aggregate of its own: it cannot exist without the
/// project, it is edited in the same screen by the same people, and the rule "a live project needs a
/// signed contract" is a field check here instead of a read model lookup that lags behind the
/// async daemon.
/// </summary>
public sealed record ProjectContract(
    string ContractNumber,
    ContractStatus Status,
    DateOnly? SignedOn,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    ContactPerson? SignedBy,
    CompanyPartySnapshot Party,
    Guid? DocumentId
)
{
    public bool IsSigned => Status is ContractStatus.Signed;
}
