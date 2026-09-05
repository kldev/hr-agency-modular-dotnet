using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Application.JobPosting.Create;

public interface IJobPostData
{
    string Title { get; }
    string? Summary { get; }
    string Description { get; }
    IReadOnlyList<string> Responsibilities { get; }
    IReadOnlyList<string> Requirements { get; }
    IReadOnlyList<string> Skills { get; }
    string Location { get; }
    string CountryCode { get; }
    // ReSharper disable once UnusedMemberInSuper.Global
    EmploymentType EmploymentType { get; }
    // ReSharper disable once UnusedMemberInSuper.Global
    WorkMode WorkMode { get; }
    
    string LanguageCode { get; }
    CurrencyCode CurrencyCode { get; }
    
    decimal SalaryMin { get; }
    decimal SalaryMax { get; }
}