using HrAgencySystem.Company.Application.Create;
using HrAgencySystem.Company.Events;
using HrAgencySystem.Company.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Company.Application.Update;

public static class UpdateCompanyHandler
{
    [AggregateHandler]
     public static async Task<(CompanyUpdated, Wolverine.Marten.Events)> Handle(
        UpdateCompany command,
        Domain.Company aggregate,
        ICompanyService service,
        IClock clock,
        CancellationToken ct)
     {
         var user = await service.GetUserAsync(command.ModifiedBy, ct);
         var (data, _) = CompanyDataFactory.CreateCompanyData(command);
         var (name, registrationNumber, webSite, countryCode) = data;
         if (aggregate.OrganizationId.Value != command.OrganizationId)
             throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);

         var @event = new CompanyUpdated(command.CompanyId, command.OrganizationId,
             name.Value,
             command.Industry,
             webSite.Value,
             registrationNumber.Value,
             countryCode.Value,
             user,
             clock.UtcNow);

         return (@event, [@event]);
     }
}