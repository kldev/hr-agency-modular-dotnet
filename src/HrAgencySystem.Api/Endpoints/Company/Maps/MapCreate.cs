using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Company.Application.Commands;
using HrAgencySystem.Company.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapCreate
{
    internal static void Map(
        RouteGroupBuilder endpoints)
    {
        endpoints.MapPost("/api/companies", Handler)
            .WithSummary("Create company")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);;
    }
    
    private static async Task<IResult> Handler(AppUserAuthenticated user, CreateCompanyRequest request,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result =
            await bus.InvokeAsync<CompanyCreated>(
                request.ToCommand(user.GetOrganization),
                ct);

        return TypedResults.Created(
            $"/api/companies/{result.CompanyId}",
            result);
    }

    internal record CreateCompanyRequest(
        string Name,
        string CountryCode,
        string TaxId,
        string RegistrationNumber)
    {
        public CreateCompany ToCommand(OrganizationId  organizationId)
        {
            return new CreateCompany(organizationId.Value, Name, CountryCode, TaxId, RegistrationNumber);
        }
    }
}