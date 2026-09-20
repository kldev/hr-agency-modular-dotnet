using HrAgencySystem.LegalEntities.Application.Create;
using HrAgencySystem.LegalEntities.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.UnitTests.LegalEntities;

public class LegalEntityDataFactoryTests
{
    [Fact]
    public void Create_WithCompleteData_BuildsTheEntity()
    {
        var data = LegalEntityDataFactory.Create(Data());

        Assert.Equal("HR Agency", data.Name.Value);
        Assert.Equal("HR Agency sp. z o.o.", data.LegalName.Value);
        Assert.Equal("5213870274", data.TaxId.Value);
        Assert.Equal("PL5213870274", data.VatNumber.Value);
        Assert.Equal("Warszawa", data.RegisteredAddress.City);
        Assert.Equal("Anna", data.President.FirstName);
        Assert.Equal("Nowak", data.President.LastName);
    }

    [Fact]
    public void Create_WithoutAPresidentEmail_IsFine()
    {
        var data = LegalEntityDataFactory.Create(Data(presidentEmail: null));

        Assert.Null(data.President.Email);
    }

    [Fact]
    public void Create_WithAMalformedPresidentEmail_Throws()
    {
        var exception = Assert.Throws<ValidationException>(() =>
            LegalEntityDataFactory.Create(Data(presidentEmail: "not-an-address"))
        );

        Assert.Contains(Email.InvalidEmail, exception.Errors);
    }

    /// <summary>
    /// Every fault in one answer: somebody correcting a form should not have to submit it five
    /// times to be told about five things.
    /// </summary>
    [Fact]
    public void Create_ReportsEveryFaultAtOnce()
    {
        var exception = Assert.Throws<ValidationException>(() =>
            LegalEntityDataFactory.Create(Data(name: "", taxId: "", presidentFirstName: ""))
        );

        Assert.True(exception.Errors.Count >= 3);
    }

    [Fact]
    public void Create_WhenTradingEndsBeforeItBegan_Throws()
    {
        var exception = Assert.Throws<ValidationException>(() =>
            LegalEntityDataFactory.Create(
                Data(activeFrom: new DateOnly(2026, 5, 1), activeTo: new DateOnly(2026, 4, 30))
            )
        );

        Assert.Contains(LegalEntityDataFactory.ClosedBeforeOpenedMessage, exception.Errors);
    }

    [Fact]
    public void Create_WithTheSameDayOpenedAndClosed_IsFine()
    {
        var day = new DateOnly(2026, 5, 1);

        var data = LegalEntityDataFactory.Create(Data(activeFrom: day, activeTo: day));

        Assert.NotNull(data);
    }

    [Fact]
    public void Create_KeepsOneAccountPerPurposeAndCurrency()
    {
        var data = LegalEntityDataFactory.Create(
            Data(
                accounts:
                [
                    Account(BankAccountPurpose.Incoming, CurrencyCode.PLN),
                    Account(BankAccountPurpose.Incoming, CurrencyCode.EUR),
                    Account(BankAccountPurpose.Outgoing, CurrencyCode.PLN),
                ]
            )
        );

        Assert.Equal(3, data.BankAccounts.Count);
    }

    /// <summary>
    /// "Which account goes on a EUR invoice" has to have one answer, so the second one is refused
    /// rather than silently shadowed.
    /// </summary>
    [Fact]
    public void Create_WithTwoAccountsForTheSamePurposeAndCurrency_Throws()
    {
        var exception = Assert.Throws<ValidationException>(() =>
            LegalEntityDataFactory.Create(
                Data(
                    accounts:
                    [
                        Account(BankAccountPurpose.Incoming, CurrencyCode.EUR),
                        Account(
                            BankAccountPurpose.Incoming,
                            CurrencyCode.EUR,
                            "DE89370400440532013001"
                        ),
                    ]
                )
            )
        );

        Assert.Contains(LegalEntityBankAccount.DuplicateMessage, exception.Errors);
    }

    [Fact]
    public void Create_WithAMalformedIban_Throws()
    {
        var exception = Assert.Throws<ValidationException>(() =>
            LegalEntityDataFactory.Create(
                Data(accounts: [Account(BankAccountPurpose.Incoming, CurrencyCode.PLN, "123")])
            )
        );

        Assert.Contains(LegalEntityBankAccount.InvalidIbanMessage, exception.Errors);
    }

    /// <summary>A number written the way a bank prints it is the same number.</summary>
    [Fact]
    public void Create_NormalisesSpacingInAnIban()
    {
        var data = LegalEntityDataFactory.Create(
            Data(
                accounts:
                [
                    Account(
                        BankAccountPurpose.Incoming,
                        CurrencyCode.PLN,
                        "pl61 1090 1014 0000 0712 1981 2874"
                    ),
                ]
            )
        );

        Assert.Equal("PL61109010140000071219812874", data.BankAccounts[0].Iban);
    }

    private static BankAccountData Account(
        BankAccountPurpose purpose,
        CurrencyCode currency,
        string iban = "PL61109010140000071219812874"
    )
    {
        return new BankAccountData(purpose, currency, iban, null, "Bank Millennium");
    }

    private static TestData Data(
        string name = "HR Agency",
        string taxId = "5213870274",
        string presidentFirstName = "Anna",
        string? presidentEmail = "anna.nowak@hr-agency.com",
        DateOnly? activeFrom = null,
        DateOnly? activeTo = null,
        IReadOnlyList<BankAccountData>? accounts = null
    )
    {
        return new TestData(
            name,
            "HR Agency sp. z o.o.",
            taxId,
            "PL5213870274",
            "Długa",
            "33",
            null,
            "00-001",
            "Warszawa",
            "PL",
            "The one that posts people to Germany.",
            presidentFirstName,
            "Nowak",
            presidentEmail,
            activeFrom ?? new DateOnly(2020, 1, 1),
            activeTo,
            accounts ?? []
        );
    }

    private sealed record TestData(
        string Name,
        string LegalName,
        string TaxId,
        string? VatNumber,
        string Street,
        string BuildingNumber,
        string? UnitNumber,
        string PostalCode,
        string City,
        string CountryCode,
        string? Description,
        string PresidentFirstName,
        string PresidentLastName,
        string? PresidentEmail,
        DateOnly ActiveFrom,
        DateOnly? ActiveTo,
        IReadOnlyList<BankAccountData> BankAccounts
    ) : ILegalEntityData;
}
