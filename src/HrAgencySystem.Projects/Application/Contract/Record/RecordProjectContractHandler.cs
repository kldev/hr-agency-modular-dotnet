using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.Contract.Record;

public static class RecordProjectContractHandler
{
    public const string ContractNumberRequiredMessage = "Contract number is required.";
    public const string ContractNumberMaxLengthMessage =
        "Contract number cannot exceed 100 characters.";
    public const string ValidToBeforeValidFromMessage =
        "The contract cannot expire before it takes effect.";
    public const string SignedAfterValidFromMessage =
        "A contract cannot take effect before it was signed.";
    public const string SignedOnRequiredMessage = "A signed contract needs a signature date.";
    public const string PartyProfileRequiredMessage =
        "The client company profile is incomplete: a contract needs a legal name and a registered address.";

    private const int ContractNumberMaxLength = 100;

    [AggregateHandler]
    public static async Task<(ProjectContractRecorded, Wolverine.Marten.Events)> Handle(
        RecordProjectContract command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var contractNumber = ValidateDates(command);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var company = await service.GetCompanyAsync(
            OrganizationId.From(command.OrganizationId),
            aggregate.Company.Id,
            ct
        );

        // The party is frozen here and nowhere else. A contract states who signed it and under which
        // address; if the company is later renamed or moves, the contract still says what it said.
        var party = FreezeParty(company);

        var @event = new ProjectContractRecorded(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            new ProjectContract(
                contractNumber,
                command.Status,
                command.SignedOn,
                command.ValidFrom,
                command.ValidTo,
                aggregate.ContactInRole(ContactRole.ContractSignatory)?.Person,
                party,
                aggregate.Contract?.DocumentId
            ),
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }

    private static CompanyPartySnapshot FreezeParty(CompanySnapshot company)
    {
        if (!company.IsProfileComplete || company.RegisteredAddress is null)
            throw new BusinessRuleException(PartyProfileRequiredMessage);

        return new CompanyPartySnapshot(
            company.ContractName,
            company.TaxId,
            company.VatNumber,
            company.RegisteredAddress
        );
    }

    private static string ValidateDates(RecordProjectContract command)
    {
        var errors = new List<string>();

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        var number = (command.ContractNumber ?? "").Trim();
        if (number.Length == 0)
            errors.Add(ContractNumberRequiredMessage);
        else if (number.Length > ContractNumberMaxLength)
            errors.Add(ContractNumberMaxLengthMessage);

        if (command.ValidTo is not null && command.ValidTo < command.ValidFrom)
            errors.Add(ValidToBeforeValidFromMessage);

        if (command.SignedOn is not null && command.SignedOn > command.ValidFrom)
            errors.Add(SignedAfterValidFromMessage);

        if (command.Status is ContractStatus.Signed && command.SignedOn is null)
            errors.Add(SignedOnRequiredMessage);

        return errors.Count > 0 ? throw new ValidationException(errors) : number;
    }
}
