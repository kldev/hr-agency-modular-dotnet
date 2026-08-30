using HrAgencySystem.Company.Application.Commands;
using HrAgencySystem.Company.Events;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapCreateCompany
{
    public static void Map(
        RouteGroupBuilder endpoints)
    {
        endpoints.MapPost(
            "",
            async (
                CreateCompanyRequest request,
                IMessageBus bus,
                CancellationToken ct) =>
            {
                var result =
                    await bus.InvokeAsync<CompanyCreated>(
                        request.ToCommand(),
                        ct);

                return TypedResults.Created(
                    $"/api/companies/{result.CompanyId}",
                    result);
            });
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