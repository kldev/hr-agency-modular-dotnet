using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.JobDescription.Application.Commands;

public interface IJobDescription
{
    string Title { get; }
    string? Summary { get; }
    string Description { get; }
    IReadOnlyList<string> Responsibilities { get; }
    IReadOnlyList<string> Requirements { get; }
    IReadOnlyList<string> Skills { get; }
    string Location { get; }
    string CountryCode { get; }
    EmploymentType EmploymentType { get; }
    WorkMode WorkMode { get; }
    CurrencyCode CurrencyCode { get; }
    decimal SalaryMin { get; }
    decimal SalaryMax { get; }
}