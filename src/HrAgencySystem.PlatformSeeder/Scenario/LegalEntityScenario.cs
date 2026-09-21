using HrAgencySystem.LegalEntities.Application.Create;
using HrAgencySystem.LegalEntities.Domain.ValueObjects;
using HrAgencySystem.LegalEntities.Events;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

/// <summary>
/// The agency's own companies - the ones that post people, not the clients who receive them. Two of
/// them on purpose: which of our entities posted somebody is frozen on their assignment, so a demo
/// with a single entity would never show that the answer can differ.
/// </summary>
internal sealed class LegalEntityScenario(IMessageBus bus)
{
    internal sealed record LegalEntityData(Guid LegalEntityId, string Name);

    private sealed record Spec(
        string Name,
        string LegalName,
        string TaxId,
        string VatNumber,
        string Street,
        string BuildingNumber,
        string PostalCode,
        string City,
        string PresidentFirstName,
        string PresidentLastName,
        string Iban
    );

    private static readonly Spec[] Specs =
    [
        new(
            "HR Agency Poland",
            "HR Agency Poland sp. z o.o.",
            "7010882345",
            "PL7010882345",
            "Prosta",
            "51",
            "00-838",
            "Warszawa",
            "Marta",
            "Lewandowska",
            "PL61109010140000071219812874"
        ),
        new(
            "HR Agency Delegowanie",
            "HR Agency Delegowanie sp. z o.o.",
            "5252766411",
            "PL5252766411",
            "Grzybowska",
            "62",
            "00-844",
            "Warszawa",
            "Rafał",
            "Adamczyk",
            "PL27114020040000300201355387"
        ),
    ];

    internal async Task<IReadOnlyList<LegalEntityData>> Create(
        Guid organizationId,
        Guid createdBy,
        DateOnly today
    )
    {
        var created = new List<LegalEntityData>();

        foreach (var spec in Specs)
        {
            var command = new CreateLegalEntity(
                organizationId,
                spec.Name,
                spec.LegalName,
                spec.TaxId,
                spec.VatNumber,
                spec.Street,
                spec.BuildingNumber,
                null,
                spec.PostalCode,
                spec.City,
                "PL",
                "Seeded delivering entity.",
                spec.PresidentFirstName,
                spec.PresidentLastName,
                null,
                today.AddYears(-6),
                null,
                [
                    new BankAccountData(
                        BankAccountPurpose.Incoming,
                        CurrencyCode.PLN,
                        spec.Iban,
                        null,
                        null
                    ),
                ],
                createdBy
            );

            var result = await bus.InvokeAsync<LegalEntityCreated>(command);

            created.Add(new LegalEntityData(result.LegalEntityId, result.Name));
        }

        return created;
    }
}
