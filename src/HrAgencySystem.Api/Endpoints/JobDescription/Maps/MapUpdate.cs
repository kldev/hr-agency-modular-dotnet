using HrAgencySystem.Api.Auth;
using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobDescription.Maps;

internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/api/job-description/{jobDescriptionId}", Handler).WithSummary("Update job description");
    }
    
    private static async Task<IResult> Handler(IMessageBus bus, AppUserAuthenticated user, Guid jobDescriptionId,
        UpdateJobDescriptionRequest request, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<JobDescriptionUpdated>(request.ToCommand(jobDescriptionId, user.OrganizationId, user.UserId), ct);
        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record UpdateJobDescriptionRequest(string Title,
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
    decimal SalaryMax)
{
    public UpdateJobDescription ToCommand(Guid jobDescriptionId, Guid organizationId, Guid modifiedBy) =>
        new (jobDescriptionId, 
            organizationId, 
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
            modifiedBy);
}