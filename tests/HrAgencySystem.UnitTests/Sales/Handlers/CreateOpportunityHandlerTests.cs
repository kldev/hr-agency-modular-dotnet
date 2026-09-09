using HrAgencySystem.Sales.Application.Opportunities.Create;
using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Sales.Handlers;

public class CreateOpportunityHandlerTests : BaseTest
{
    private readonly IDocumentSession _documentSession =
        Substitute.For<IDocumentSession>();


    private readonly ISalesService _salesService =
        Substitute.For<ISalesService>();

    private readonly IClock _clock =
        Substitute.For<IClock>();

    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid CreatedById = Guid.NewGuid();
    private static readonly Guid OwnerId = Guid.NewGuid();

    private static UserSnapshot CreatedBy { get; } =
        new(
            CreatedById,
            "Bob",
            "Smith",
            "bob-smith@hr-agency.com");

    private static UserSnapshot Owner { get; } =
        new(
            OwnerId,
            "Alice",
            "Wells",
            "alice-wells@hr-agency.com");

    private static CompanySnapshot Company { get; } =
        new(
            CompanyId,
            "Company A",
            "TX-100-101");

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSalesOpportunityCreated()
    {
        var now = new DateTimeOffset(
            2026,
            8,
            30,
            10,
            0,
            0,
            TimeSpan.Zero);

        var expectedCloseDate = new DateTimeOffset(
            2026,
            12,
            31,
            0,
            0,
            0,
            TimeSpan.Zero);

        var command = CreateValidCommand(
            organizationId: OrganizationId,
            companyId: CompanyId,
            createdBy: CreatedById,
            ownerId: OwnerId,
            expectedCloseDate: expectedCloseDate,
            isHotLead: true);

        SetupOrganization();
        SetupCreatedBy();
        SetupOwner();
        SetupCompany();

        _clock.UtcNow.Returns(now);

        var result = await Handle(command);

        Assert.NotEqual(Guid.Empty, result.OpportunityId);
        Assert.Equal(OrganizationId, result.OrganizationId);
        Assert.Equal(CompanyId, result.Company.Id);

        Assert.True(result.IsHotLead);
        
        Assert.Equal(
            "Senior .NET Developer",
            result.Title);

        Assert.Equal(
            "Potential software development opportunity.",
            result.Description);

        Assert.Equal(
            SalesOpportunityStage.New,
            result.Stage);

        Assert.Equal(
            50000m,
            result.ExpectedValue);

        Assert.Equal(
            CurrencyCode.PLN,
            result.Currency);

        Assert.Equal(
            expectedCloseDate,
            result.ExpectedCloseDate);

        Assert.Equal(
            OwnerId,
            result.Responsible.Id);

        Assert.Equal(
            CreatedById,
            result.CreatedBy.Id);

        Assert.Equal(
            now,
            result.CreatedAt);

        await _salesService
            .Received(1)
            .ValidateOrganization(
                OrganizationId,
                Arg.Any<CancellationToken>());

        await _salesService
            .Received(1)
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>());

        await _salesService
            .Received(1)
            .GetUserAsync(
                OwnerId,
                Arg.Any<CancellationToken>());

        await _salesService
            .Received(1)
            .GetCompanyAsync(
                CompanyId,
                Arg.Any<CancellationToken>());

        _documentSession.Events
            .Received(1)
            .StartStream<SalesOpportunity>(
                OrganizationId,
                Arg.Is<OpportunityCreated>(x =>
                    x.OpportunityId == result.OpportunityId &&
                    x.OrganizationId == result.OrganizationId &&
                    x.Company.Id == result.Company.Id &&
                    x.Title == result.Title &&
                    x.Description == result.Description &&
                    x.Stage == result.Stage &&
                    x.ExpectedValue == result.ExpectedValue &&
                    x.Currency == result.Currency &&
                    x.ExpectedCloseDate == result.ExpectedCloseDate &&
                    x.Responsible.Id == result.Responsible.Id &&
                    x.CreatedBy.Id == result.CreatedBy.Id &&
                    x.CreatedAt == result.CreatedAt));
    }

    [Fact]
    public async Task Handle_WithOwnerIdNull_UsesCreatedByAsOwner()
    {
        var command = CreateValidCommand(
            organizationId: OrganizationId,
            companyId: CompanyId,
            createdBy: CreatedById,
            ownerId: null);

        SetupOrganization();
        SetupCreatedBy();
        SetupCompany();

        var result = await Handle(command);

        Assert.Equal(
            CreatedById,
            result.Responsible.Id);

        await _salesService
            .Received(1)
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>());

        await _salesService
            .DidNotReceive()
            .GetUserAsync(
                Arg.Is<Guid>(id => id != CreatedById),
                Arg.Any<CancellationToken>());

        AssertStreamCreated();
    }

    [Fact]
    public async Task Handle_WithOwnerIdEqualToCreatedBy_UsesCreatedByAsOwner()
    {
        var command = CreateValidCommand(
            organizationId: OrganizationId,
            companyId: CompanyId,
            createdBy: CreatedById,
            ownerId: CreatedById);

        SetupOrganization();
        SetupCreatedBy();
        SetupCompany();

        var result = await Handle(command);

        Assert.Equal(
            CreatedById,
            result.Responsible.Id);

        await _salesService
            .Received(1)
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>());

        AssertStreamCreated();
    }

    [Fact]
    public async Task Handle_WithDifferentOwnerId_LoadsSpecifiedOwner()
    {
        var command = CreateValidCommand(
            organizationId: OrganizationId,
            companyId: CompanyId,
            createdBy: CreatedById,
            ownerId: OwnerId);

        SetupOrganization();
        SetupCreatedBy();
        SetupOwner();
        SetupCompany();

        var result = await Handle(command);

        Assert.Equal(
            OwnerId,
            result.Responsible.Id);

        Assert.Equal(
            "Alice",
            result.Responsible.FirstName);

        Assert.Equal(
            "Wells",
            result.Responsible.LastName);

        await _salesService
            .Received(1)
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>());

        await _salesService
            .Received(1)
            .GetUserAsync(
                OwnerId,
                Arg.Any<CancellationToken>());

        AssertStreamCreated();
    }

    [Fact]
    public async Task Handle_WithInvalidTitle_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            title: "");

        var exception = await Assert.ThrowsAsync<ValidationException>(() => Handle(command));

        Assert.NotEmpty(exception.Errors);

        AssertNoOrganizationValidation();
        AssertNoUserLookup();
        AssertNoCompanyLookup();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithEmptyDescription_CreatesOpportunity()
    {
        var command = CreateValidCommand(
            description: "");

        var result = await Handle(command);
    
        Assert.Empty(result.Description);
        
    }

    [Fact]
    public async Task Handle_WithInvalidTitleAndEmptyDescription_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            title: "",
            description: "");

        var exception = await Assert.ThrowsAsync<ValidationException>(() => Handle(command));

        Assert.NotEmpty(exception.Errors);
        Assert.Single(exception.Errors);

        AssertNoOrganizationValidation();
        AssertNoUserLookup();
        AssertNoCompanyLookup();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithNonExistingOrganization_ThrowsBusinessRuleException()
    {
        var exceptionToThrow = new BusinessRuleException(
            IOrganizationChecker.OrganizationCheckMessage);

        _salesService
            .ValidateOrganization(
                OrganizationId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(exceptionToThrow));

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => Handle(
            CreateValidCommand(
                organizationId: OrganizationId)));

        Assert.Equal(
            IOrganizationChecker.OrganizationCheckMessage,
            exception.Message);

        await _salesService
            .Received(1)
            .ValidateOrganization(
                OrganizationId,
                Arg.Any<CancellationToken>());

        AssertNoUserLookup();
        AssertNoCompanyLookup();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithNonExistingCreatedByUser_ThrowsBusinessRuleException()
    {
        var exceptionToThrow = new BusinessRuleException(
            "User was not found.");

        SetupOrganization();

        _salesService
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<UserSnapshot>(exceptionToThrow));

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => Handle(
            CreateValidCommand(
                organizationId: OrganizationId,
                createdBy: CreatedById)));

        Assert.Equal(
            exceptionToThrow.Message,
            exception.Message);

        await _salesService
            .Received(1)
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>());

        AssertNoCompanyLookup();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithNonExistingOwner_ThrowsBusinessRuleException()
    {
        var exceptionToThrow = new BusinessRuleException(
            "User was not found.");

        SetupOrganization();
        SetupCreatedBy();

        _salesService
            .GetUserAsync(
                OwnerId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<UserSnapshot>(exceptionToThrow));

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => Handle(
            CreateValidCommand(
                organizationId: OrganizationId,
                createdBy: CreatedById,
                ownerId: OwnerId)));

        Assert.Equal(
            exceptionToThrow.Message,
            exception.Message);

        await _salesService
            .Received(1)
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>());

        await _salesService
            .Received(1)
            .GetUserAsync(
                OwnerId,
                Arg.Any<CancellationToken>());

        AssertNoCompanyLookup();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithNonExistingCompany_ThrowsBusinessRuleException()
    {
        var exceptionToThrow = new BusinessRuleException(
            "Company was not found.");

        SetupOrganization();
        SetupCreatedBy();

        _salesService
            .GetCompanyAsync(
                CompanyId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CompanySnapshot>(exceptionToThrow));

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => Handle(
            CreateValidCommand(
                organizationId: OrganizationId,
                companyId: CompanyId)));

        Assert.Equal(
            exceptionToThrow.Message,
            exception.Message);

        await _salesService
            .Received(1)
            .GetCompanyAsync(
                CompanyId,
                Arg.Any<CancellationToken>());

        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithInvalidOwnerId_UsesCreatedByAsOwner()
    {
        var command = CreateValidCommand(
            organizationId: OrganizationId,
            companyId: CompanyId,
            createdBy: CreatedById,
            ownerId: Guid.Empty);

        SetupOrganization();
        SetupCreatedBy();
        SetupCompany();

        var result = await Handle(command);

        Assert.Equal(
            CreatedById,
            result.Responsible.Id);

        await _salesService
            .Received(1)
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>());

        await _salesService
            .DidNotReceive()
            .GetUserAsync(
                Arg.Is<Guid>(id => id == Guid.Empty),
                Arg.Any<CancellationToken>());

        AssertStreamCreated();
    }

    private async Task<OpportunityCreated> Handle(
        CreateOpportunity command,
        IClock? clock = null)
    {
        return await CreateOpportunityHandler.Handle(
            command,
            _salesService,
            _documentSession,
            clock ?? _clock,
            CancellationToken.None);
    }

    private void SetupOrganization()
    {
        _salesService
            .ValidateOrganization(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }

    private void SetupCreatedBy()
    {
        _salesService
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>())
            .Returns(CreatedBy);
    }

    private void SetupOwner()
    {
        _salesService
            .GetUserAsync(
                OwnerId,
                Arg.Any<CancellationToken>())
            .Returns(Owner);
    }

    private void SetupCompany()
    {
        _salesService
            .GetCompanyAsync(
                CompanyId,
                Arg.Any<CancellationToken>())
            .Returns(Company);
    }

    private void AssertNoOrganizationValidation()
    {
        _salesService
            .DidNotReceive()
            .ValidateOrganization(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    private void AssertNoUserLookup()
    {
        _salesService
            .DidNotReceive()
            .GetUserAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    private void AssertNoCompanyLookup()
    {
        _salesService
            .DidNotReceive()
            .GetCompanyAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    private void AssertNoStream()
    {
        _documentSession.Events
            .DidNotReceive()
            .StartStream<SalesOpportunity>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }

    private void AssertStreamCreated()
    {
        _documentSession.Events
            .Received(1)
            .StartStream<SalesOpportunity>(
                Arg.Any<Guid>(),
                Arg.Any<OpportunityCreated>());
    }

    private static CreateOpportunity CreateValidCommand(
        Guid? organizationId = null,
        Guid? companyId = null,
        string title = "Senior .NET Developer",
        string description = "Potential software development opportunity.",
        decimal expectedValue = 50000m,
        CurrencyCode currency = CurrencyCode.PLN,
        DateTimeOffset? expectedCloseDate = null,
        Guid? ownerId = null,
        Guid? createdBy = null,
        bool? isHotLead = null)
    {
        return new CreateOpportunity(
            organizationId ?? OrganizationId,
            companyId ?? CompanyId,
            title,
            description,
            expectedValue,
            isHotLead ?? false,
            currency,
            expectedCloseDate ??
            new DateTimeOffset(
                2026,
                12,
                31,
                0,
                0,
                0,
                TimeSpan.Zero),
            ownerId,
            createdBy ?? CreatedById);
    }
}
