using HrAgencySystem.Api.Auth;
using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobDescription.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/api/job-description", Handler).WithSummary("Create job description");
    }

    private static async Task<IResult> Handler(IMessageBus bus, AppUserAuthenticated user,
        CreateJobDescriptionRequest request, CancellationToken ct)
    {
        var result =
            await bus.InvokeAsync<JobDescriptionCreated>(request.ToCommand(user.OrganizationId, user.UserId), ct);
        return TypedResults.Created($"/api/job-description/{result.JobDescriptionId}", result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record CreateJobDescriptionRequest(
    Guid CompanyId,
    string Title,
    string? Summary,
    string Description,
    IReadOnlyList<string> Responsibilities,
    IReadOnlyList<string> Requirements,
    IReadOnlyList<string> Skills,
    string Location,
    string CountryCode,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    CurrencyCode  CurrencyCode,
    decimal SalaryMin,
    decimal SalaryMax,
    Guid RecruiterId)
{
    public CreateJobDescription ToCommand(Guid organizationId, Guid createdBy) =>
        new ( 
            organizationId, 
            CompanyId,
            Title, 
            Summary, 
            Description,
            Responsibilities,
            Requirements,
            Skills,
            Location,
            CountryCode,
            EmploymentType,
            WorkMode,
            CurrencyCode,
            SalaryMin,
            SalaryMax,
            RecruiterId,
            createdBy);
}