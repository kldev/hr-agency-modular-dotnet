namespace HrAgencySystem.Company.Infrastructure.Persistence;

public sealed record CompanyTaxIdReservation(Guid Id, Guid OrganizationId, string TaxId, Guid CompanyId);