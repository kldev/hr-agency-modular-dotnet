using HrAgencySystem.Company.Application.CompleteProfile;
using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Domain.ValueObjects;
using HrAgencySystem.Company.Events;
using HrAgencySystem.Company.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.SharedKernel.Web.Common;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Companies.Handlers;

public class CompleteCompanyProfileHandlerTests : BaseTest
{
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    private static readonly UserSnapshot User = new(
        UserId,
        "Alice",
        "Wells",
        "alice-wells@hr-agency.com"
    );

    private readonly ICompanyService _service = Substitute.For<ICompanyService>();

    public CompleteCompanyProfileHandlerTests()
    {
        _service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(User);
    }

    [Fact]
    public async Task Handle_WithFullProfile_ReturnsProfileUpdated()
    {
        var (result, _) = await Handle(Command());

        Assert.Equal(CompanyId, result.CompanyId);
        Assert.Equal("ACME Corporation sp. z o.o.", result.Profile.LegalName);
        Assert.Equal("PL1234567890", result.Profile.VatNumber);
        Assert.Equal("PL61109010140000071219812874", result.Profile.Iban);
        Assert.NotNull(result.Profile.RegisteredAddress);
        Assert.Equal("Warszawa", result.Profile.RegisteredAddress!.City);
        Assert.Equal("PL", result.Profile.RegisteredAddress.CountryCode);
        Assert.True(result.Profile.IsComplete);
    }

    [Fact]
    public async Task Handle_WhenProfileBecomesComplete_AlsoRaisesProfileCompleted()
    {
        var (_, events) = await Handle(Command());

        Assert.Equal(2, events.Count);
        var completed = Assert.IsType<CompanyProfileCompleted>(events[1]);
        Assert.Equal(CompanyId, completed.CompanyId);
        Assert.Equal(UserId, completed.CompletedBy.Id);
    }

    [Fact]
    public async Task Handle_WhenProfileWasAlreadyComplete_DoesNotRepeatProfileCompleted()
    {
        var aggregate = Aggregate();
        aggregate.Apply(
            new CompanyProfileUpdated(
                CompanyId,
                OrganizationId,
                CompleteProfile(),
                User,
                DateTimeOffset.UtcNow
            )
        );

        var (_, events) = await Handle(Command(), aggregate);

        // Saving the form again is not news, even when the form is still complete.
        Assert.Single(events);
    }

    [Fact]
    public async Task Handle_WithPartialProfile_DoesNotRaiseProfileCompleted()
    {
        var command = Command() with
        {
            Street = null,
            BuildingNumber = null,
            UnitNumber = null,
            PostalCode = null,
            City = null,
            CountryCode = null,
        };

        var (result, events) = await Handle(command);

        Assert.False(result.Profile.IsComplete);
        Assert.Single(events);
    }

    [Fact]
    public async Task Handle_WithEmptyProfile_IsAccepted()
    {
        // Clearing the form is a legitimate edit; it just takes the company back out of "contractable".
        var (result, events) = await Handle(Empty());

        Assert.Null(result.Profile.LegalName);
        Assert.Null(result.Profile.RegisteredAddress);
        Assert.Single(events);
    }

    [Fact]
    public async Task Handle_WithForeignOrganization_Throws()
    {
        var command = Command() with { OrganizationId = Guid.NewGuid() };

        await Assert.ThrowsAsync<OrganizationAccessDeniedException>(() => Handle(command));
    }

    [Fact]
    public async Task Handle_WithHalfAnAddress_ThrowsValidation()
    {
        var command = Command() with { City = null, PostalCode = null };

        var error = await Assert.ThrowsAsync<ValidationException>(() => Handle(command));

        Assert.Contains(PostalAddress.PartialMessage, error.Errors);
    }

    [Fact]
    public async Task Handle_WithInvalidVatNumber_ThrowsValidation()
    {
        var command = Command() with { VatNumber = "1234567890" };

        var error = await Assert.ThrowsAsync<ValidationException>(() => Handle(command));

        Assert.Contains(VatNumber.InvalidFormatMessage, error.Errors);
    }

    [Fact]
    public async Task Handle_WithBicButNoIban_ThrowsValidation()
    {
        var command = Command() with { Iban = null, Bic = "BREXPLPW" };

        var error = await Assert.ThrowsAsync<ValidationException>(() => Handle(command));

        Assert.Contains(BankAccount.BicWithoutIbanMessage, error.Errors);
    }

    [Fact]
    public async Task Handle_CollectsEveryProblemAtOnce()
    {
        var command = Command() with { VatNumber = "!!", Iban = "too-short", City = null };

        var error = await Assert.ThrowsAsync<ValidationException>(() => Handle(command));

        Assert.Equal(3, error.Errors.Count);
    }

    private async Task<(CompanyProfileUpdated, Wolverine.Marten.Events)> Handle(
        CompleteCompanyProfile command,
        HrAgencySystem.Company.Domain.Company? aggregate = null
    )
    {
        return await CompleteCompanyProfileHandler.Handle(
            command,
            aggregate ?? Aggregate(),
            _service,
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );
    }

    private static HrAgencySystem.Company.Domain.Company Aggregate()
    {
        var company = HrAgencySystem.Company.Domain.Company.Empty();
        company.Apply(
            new CompanyCreated(
                CompanyId,
                OrganizationId,
                "ACME Corporation",
                "PL",
                "PL1234567890",
                "REG-1",
                Industry.Accounting,
                "https://acme.example.com",
                User,
                DateTimeOffset.UtcNow
            )
        );

        return company;
    }

    private static CompanyProfile CompleteProfile() =>
        new(
            "ACME Corporation sp. z o.o.",
            PostalAddress.Create("Prosta", "51", null, "00-838", "Warszawa", "PL"),
            "PL1234567890",
            null,
            null,
            null
        );

    private static CompleteCompanyProfile Command() =>
        new(
            CompanyId,
            OrganizationId,
            "  ACME Corporation sp. z o.o.  ",
            "Prosta",
            "51",
            "12",
            "00-838",
            "Warszawa",
            "pl",
            " pl 123 456 78 90 ",
            "PL61 1090 1014 0000 0712 1981 2874",
            "WBKPPLPP",
            new ContactPerson("jan@acme.example.com", "Jan", "Kowalski", "CEO", "+48 600 100 200"),
            UserId
        );

    private static CompleteCompanyProfile Empty() =>
        new(
            CompanyId,
            OrganizationId,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            UserId
        );
}
