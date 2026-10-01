using HrAgencySystem.Api.Endpoints.LegalEntity.Maps;
using HrAgencySystem.LegalEntities.Application.Create;
using HrAgencySystem.LegalEntities.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.IntegrationTests.LegalEntities;

internal static class LegalEntityTestData
{
    private static readonly Random Random = new();

    internal static string NewTaxId() => Random.Next(1000000000, int.MaxValue).ToString();

    internal static LegalEntityRequest Request(
        string? name = null,
        string? taxId = null,
        DateOnly? activeFrom = null,
        DateOnly? activeTo = null,
        IReadOnlyList<BankAccountData>? accounts = null
    )
    {
        return new LegalEntityRequest(
            name ?? "HR Agency " + Random.Next(9999),
            "HR Agency sp. z o.o.",
            taxId ?? NewTaxId(),
            "Długa",
            "33",
            "00-001",
            "Warszawa",
            "PL",
            "Anna",
            "Nowak",
            activeFrom ?? new DateOnly(2020, 1, 1),
            accounts ?? [Account(BankAccountPurpose.Incoming, CurrencyCode.PLN)],
            VatNumber: "PL5213870274",
            Description: "The entity that posts people to Germany.",
            PresidentEmail: "anna.nowak@hr-agency.com",
            ActiveTo: activeTo
        );
    }

    internal static BankAccountData Account(
        BankAccountPurpose purpose,
        CurrencyCode currency,
        string iban = "PL61109010140000071219812874"
    )
    {
        return new BankAccountData(purpose, currency, iban, null, "Bank Millennium");
    }
}
