using HrAgencySystem.Company.Application.Commands;
using HrAgencySystem.Company.Application.Handlers;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Marten;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace HrAgencySystem.UnitTests.Company.Handlers;

public class CreateCompanyHandlerTests : BaseTest
{
    private readonly ICompanyTaxIdReservationRepository _repository =
        Substitute.For<ICompanyTaxIdReservationRepository>();
    
    private readonly IDocumentSession _documentSession =
        Substitute.For<IDocumentSession>();

    private readonly IOrganizationChecker _checker =
        Substitute.For<IOrganizationChecker>();


    [Fact]
    public async Task Handle_WithValidCommand_ReturnsCompanyCreated()
    {
        var organizationId = Guid.NewGuid();
        var now = new DateTimeOffset(
            2026, 8, 30, 10, 0, 0, TimeSpan.Zero);

        var command = new CreateCompany(
            organizationId,
            "  ACME Corporation  ",
            "pl",
            " PL123456789 ",
            " REG-123 ");

        _repository
            .ExitsAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<TaxId>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        _documentSession.Events.StartStream<HrAgencySystem.Company.Domain.Company>(Arg.Any<object>())
            .ReturnsNullForAnyArgs();

        _checker.Exists(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        
        var clock = new FixedClock(now);

        var result = await CreateCompanyHandler.Handle(
            command,
            _documentSession,
            _repository,
            clock,
            _checker,
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.CompanyId);
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal("ACME Corporation", result.Name);
        Assert.Equal("PL", result.CountryCode);
        Assert.Equal("PL123456789", result.TaxId);
        Assert.Equal("REG-123", result.RegistrationNumber);
        Assert.Equal(now, result.CreatedAt);

        await _repository.Received(1)
            .ExitsAsync(
                Arg.Is<OrganizationId>(x => x.Value == organizationId),
                Arg.Is<TaxId>(x => x.Value == "PL123456789"),
                Arg.Any<CancellationToken>());

        await _repository.Received(1)
            .ReserveAsync(
                Arg.Is<OrganizationId>(x => x.Value == organizationId),
                Arg.Is<TaxId>(x => x.Value == "PL123456789"),
                Arg.Is<CompanyId>(x => x.Value == result.CompanyId),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidData_ThrowsValidationExceptionWithAllErrors()
    {
        var command = new CreateCompany(
            Guid.NewGuid(),
            "",
            "POL",
            "",
            new string('A', 101));

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateCompanyHandler.Handle(
                command,
                _documentSession,
                _repository,
                TestClock,
                _checker,
                CancellationToken.None));

        Assert.Equal(
            [
                "Company name is required.",
                "Country code must be ISO 3166-1 alpha-2.",
                "Tax ID is required.",
                "Registration number cannot exceed 100 characters."
            ],
            exception.Errors);

        await _repository
            .DidNotReceive()
            .ExitsAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<TaxId>(),
                Arg.Any<CancellationToken>());

        await _repository
            .DidNotReceive()
            .ReserveAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<TaxId>(),
                Arg.Any<CompanyId>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidCompanyName_ThrowsValidationException()
    {
        var command = new CreateCompany(
            Guid.NewGuid(),
            "",
            "PL",
            "PL123456789",
            "REG-123");

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateCompanyHandler.Handle(
                command,
                _documentSession,
                _repository,
                TestClock,
                _checker,
                CancellationToken.None));

        Assert.Equal(
            ["Company name is required."],
            exception.Errors);

        await _repository
            .DidNotReceive()
            .ExitsAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<TaxId>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidCountryCode_ThrowsValidationException()
    {
        var command = new CreateCompany(
            Guid.NewGuid(),
            "ACME",
            "POL",
            "PL123456789",
            "REG-123");

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateCompanyHandler.Handle(
                command,
                _documentSession,
                _repository,
                TestClock,
                _checker,
                CancellationToken.None));

        Assert.Equal(
            ["Country code must be ISO 3166-1 alpha-2."],
            exception.Errors);

        await _repository
            .DidNotReceive()
            .ExitsAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<TaxId>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidTaxId_ThrowsValidationException()
    {
        var command = new CreateCompany(
            Guid.NewGuid(),
            "ACME",
            "PL",
            "",
            "REG-123");

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateCompanyHandler.Handle(
                command,
                _documentSession,
                _repository,
                TestClock,
                _checker,
                CancellationToken.None));

        Assert.Equal(
            ["Tax ID is required."],
            exception.Errors);

        await _repository
            .DidNotReceive()
            .ExitsAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<TaxId>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidRegistrationNumber_ThrowsValidationException()
    {
        var command = new CreateCompany(
            Guid.NewGuid(),
            "ACME",
            "PL",
            "PL123456789",
            new string('A', 101));

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateCompanyHandler.Handle(
                command,
                _documentSession,
                _repository,
                TestClock,
                _checker,
                CancellationToken.None));

        Assert.Equal(
            ["Registration number cannot exceed 100 characters."],
            exception.Errors);

        await _repository
            .DidNotReceive()
            .ExitsAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<TaxId>(),
                Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_NonExistingOrganizationId_ThrowsBusinessRuleException()
    {
        var organizationId = Guid.NewGuid();
        var now = new DateTimeOffset(
            2026, 8, 30, 10, 0, 0, TimeSpan.Zero);

        var command = new CreateCompany(
            organizationId,
            "  ACME Corporation  ",
            "pl",
            " PL123456789 ",
            " REG-123 ");

        _repository
            .ExitsAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<TaxId>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        _documentSession.Events.StartStream<HrAgencySystem.Company.Domain.Company>(Arg.Any<object>())
            .ReturnsNullForAnyArgs();

        _checker.Exists(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        
        var clock = new FixedClock(now);


        var exception = await Assert.ThrowsAsync<BusinessRuleException>( async () => await CreateCompanyHandler.Handle(
            command,
            _documentSession,
            _repository,
            clock,
            _checker,
            CancellationToken.None));


        Assert.Equal(typeof(BusinessRuleException), exception.GetType());
        Assert.Equal(OrganizationId.OrganizationCheckMessage, exception.Message);
    }
}