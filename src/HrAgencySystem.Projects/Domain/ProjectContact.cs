using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// A person in a role on this project. <paramref name="CompanyContactId"/> points back at the
/// client's contact list when the person came from there, and is null when somebody was typed in by
/// hand - the same shape <c>CompanyProjection</c> already uses for a primary contact.
/// </summary>
public sealed record ProjectContact(
    ContactRole Role,
    ContactPerson Person,
    Guid? CompanyContactId
);
