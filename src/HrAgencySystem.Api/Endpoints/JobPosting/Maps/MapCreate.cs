using HrAgencySystem.Api.Auth;
using HrAgencySystem.Recruitment.Application.JobPosting.Create;
using HrAgencySystem.Recruitment.Events.JobPosting;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobPosting.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/api/recruitment/job-posting", Handler).WithSummary("Creates a new job post");
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, IMessageBus bus, CreatePostRequest request, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<JobPostCreated>(request.ToCommand(user.OrganizationId, user.UserId), ct);

        return TypedResults.Created($"/api/recruitment/job-posting/{result.JobPostId}", result);
    }
}

internal sealed record CreatePostRequest(
    Guid JobDescriptionId,
    string Title,
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
    decimal SalaryMax,
    Guid RecruiterId)
{
    public CreateJobPost ToCommand(Guid organizationId, Guid createdBy) =>
        new ( 
            JobDescriptionId,
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
            RecruiterId,
            createdBy);
}