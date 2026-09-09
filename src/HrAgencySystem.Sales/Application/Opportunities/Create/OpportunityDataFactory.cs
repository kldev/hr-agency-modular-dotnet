using HrAgencySystem.Sales.Domain.Opportunity.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Application.Opportunities.Create;

internal static class OpportunityDataFactory
{
    internal static (OpportunityTitle title, LongText description) Create(IOpportunityData command)
    {
        var (title, titleError) = OpportunityTitle.TryCreate(command.Title);
        var (description, descriptionError) = LongText.TryCreate(command.Description);

        var errors = new List<string>();
        if (titleError != null) errors.Add(titleError);
        if (descriptionError != null) errors.Add(descriptionError);

        return errors.Count > 0 ? throw new ValidationException(errors) : (title!, description!);
    }
}