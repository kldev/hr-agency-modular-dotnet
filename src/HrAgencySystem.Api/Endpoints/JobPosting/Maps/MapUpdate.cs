using HrAgencySystem.Api.Auth;
using HrAgencySystem.Recruitment.Application.JobPosting.Update;
using HrAgencySystem.Recruitment.Events.JobPosting;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobPosting.Maps;


internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder group)
    {
        // PUT /api/recruitment/job-posting/{id}
        group.MapPut("{jobPostId}", Handler).WithSummary("Update job post");
    }
    
    private static async Task<IResult> Handler(IMessageBus bus, AppUserAuthenticated user, Guid jobPostId,
        UpdateJobPostRequest request, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<JobPostUpdated>(request.ToCommand(jobPostId, user.OrganizationId, user.UserId), ct);
        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record UpdateJobPostRequest(string Title,
    string? Summary,
    string Description,
    IReadOnlyList<string> Responsibilities,
    IReadOnlyList<string> Requirements,
    IReadOnlyList<string> Skills,
    string Location,
    string CountryCode,
    string LanguageCode,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    CurrencyCode  CurrencyCode,
    decimal SalaryMin,
    decimal SalaryMax)
{
    public UpdateJobPost ToCommand(Guid jobPostId, Guid organizationId, Guid modifiedBy) =>
        new (jobPostId, 
            organizationId, 
            Title, 
            Summary, 
            Description,
            Responsibilities,
            Requirements,
            Skills,
            Location,
            CountryCode,
            LanguageCode,
            EmploymentType,
            WorkMode,
            CurrencyCode,
            SalaryMin,
            SalaryMax,
            modifiedBy);
}