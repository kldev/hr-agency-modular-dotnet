using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Company.Application.Commands;
using HrAgencySystem.Company.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapCreateCompany
{
    internal static void Map(
        RouteGroupBuilder endpoints)
    {
        endpoints.MapPost("/api/companies", Handler)
            .WithSummary("Create company")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);;
    }
    
    private static async Task<IResult> Handler(CreateCompanyRequest request,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result =
            await bus.InvokeAsync<CompanyCreated>(
                request.ToCommand(),
                ct);

        return TypedResults.Created(
            $"/api/companies/{result.CompanyId}?organizationId={result.OrganizationId}",
            result);
    }

    internal record CreateCompanyRequest(
        Guid OrganizationId,
        string Name,
        string CountryCode,
        string TaxId,
        string RegistrationNumber)
    {
        public CreateCompany ToCommand()
        {
            return new CreateCompany(OrganizationId, Name, CountryCode, TaxId, RegistrationNumber);
        }
    }
}