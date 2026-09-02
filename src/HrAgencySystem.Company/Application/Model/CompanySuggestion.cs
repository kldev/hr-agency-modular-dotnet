namespace HrAgencySystem.Company.Application.Model;

public sealed record CompanySuggestion(Guid Id, string Name, string TaxNumber, string CountryCode);