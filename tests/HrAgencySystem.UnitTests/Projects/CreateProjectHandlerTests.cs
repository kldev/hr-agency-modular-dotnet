using HrAgencySystem.Compliance;
using HrAgencySystem.Projects.Application.Create;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Domain.ValueObjects;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HrAgencySystem.UnitTests.Projects;

public class CreateProjectHandlerTests : BaseTest
{
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsProjectCreated()
    {
        var now = new DateTimeOffset(2026, 9, 20, 10, 0, 0, TimeSpan.Zero);

        var result = await Handle(Command(), clock: new FixedClock(now));

        Assert.NotEqual(Guid.Empty, result.ProjectId);
        Assert.Equal(ProjectScenario.OrganizationId, result.OrganizationId);
        Assert.Equal("Delivery for ACME", result.Name);
        Assert.Equal(EngagementType.TemporaryAgencyWork, result.EngagementType);
        Assert.Equal(now, result.CreatedAt);
        Assert.Equal(ProjectScenario.UserId, result.CreatedBy.Id);

        // The company travels into the event as a snapshot, so a list of projects reads without a
        // join and a rename later does not rewrite history.
        Assert.Equal(ProjectScenario.CompanyId, result.Company.Id);
        Assert.Equal("ACME Corporation", result.Company.Name);

        _session
            .Events.Received(1)
            .StartStream<Project>(
                result.ProjectId,
                Arg.Is<ProjectCreated>(e => e.ProjectId == result.ProjectId)
            );
    }

    [Fact]
    public async Task Handle_DerivesTheWorkCountryFromTheWorkplace()
    {
        // The country drives the compliance catalogue, so it is taken from where the work happens
        // and never from where the client is registered.
        var result = await Handle(Command());

        Assert.Equal("BE", result.Placement.WorkCountry);
        Assert.Equal("Bruxelles", result.Placement.WorkplaceAddress.City);
    }

    [Fact]
    public async Task Handle_WithoutATeam_LeavesItUnassigned()
    {
        var result = await Handle(Command() with { TeamId = null });

        Assert.Null(result.TeamId);
        Assert.Null(result.TeamName);
    }

    [Fact]
    public async Task Handle_WithATeam_CarriesItsName()
    {
        var result = await Handle(Command() with { TeamId = ProjectScenario.TeamId });

        Assert.Equal(ProjectScenario.TeamId, result.TeamId);
        Assert.Equal("Tiggers", result.TeamName);
    }

    [Fact]
    public async Task Handle_AllowsAnIncompleteClientProfile()
    {
        // A project begins as a draft precisely so the paperwork and the delivery can be filled in
        // in either order. Going live is where a complete profile becomes a condition.
        var result = await Handle(
            Command(),
            ProjectScenario.Service(ProjectScenario.IncompleteCompany)
        );

        Assert.False(result.Company.IsProfileComplete);
    }

    [Fact]
    public async Task Handle_WithBlankName_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(Command() with { Name = "  " })
        );

        Assert.Contains(ProjectName.RequiredMessage, error.Errors);
    }

    [Fact]
    public async Task Handle_WithEndBeforeStart_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(Command() with { EndsOn = ProjectScenario.StartsOn.AddDays(-1) })
        );

        Assert.Contains(Placement.EndsBeforeStartMessage, error.Errors);
    }

    [Fact]
    public async Task Handle_WithBadWorkplace_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(Command() with { City = " ", CountryCode = "Belgium" })
        );

        Assert.Contains(PostalAddress.CityRequiredMessage, error.Errors);
        Assert.Contains(CountryCode.InvalidFormatMessage, error.Errors);
    }

    [Fact]
    public async Task Handle_CollectsEveryProblemAtOnce()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(
                Command() with
                {
                    Name = "",
                    City = "",
                    EndsOn = ProjectScenario.StartsOn.AddDays(-1),
                }
            )
        );

        Assert.Equal(3, error.Errors.Count);
    }

    [Fact]
    public async Task Handle_WithCompanyFromAnotherOrganization_ThrowsBusinessRule()
    {
        var service = ProjectScenario.Service();
        service
            .GetCompanyAsync(
                Arg.Any<HrAgencySystem.SharedKernel.Tenant.OrganizationId>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>()
            )
            .Throws(
                new BusinessRuleException(
                    HrAgencySystem.Projects.Services.IProjectService.CompanyNotInOrganizationMessage
                )
            );

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(Command(), service)
        );

        Assert.Equal(
            HrAgencySystem.Projects.Services.IProjectService.CompanyNotInOrganizationMessage,
            error.Message
        );
    }

    private Task<ProjectCreated> Handle(
        CreateProject command,
        HrAgencySystem.Projects.Services.IProjectService? service = null,
        IClock? clock = null
    ) =>
        CreateProjectHandler.Handle(
            command,
            service ?? ProjectScenario.Service(),
            _session,
            clock ?? TestClock,
            CancellationToken.None
        );

    private static CreateProject Command() =>
        new(
            ProjectScenario.OrganizationId,
            ProjectScenario.CompanyId,
            ProjectScenario.LegalEntityId,
            "  Delivery for ACME  ",
            "Two developers on site in Brussels.",
            EngagementType.TemporaryAgencyWork,
            "Rue de la Loi",
            "16",
            null,
            "1000",
            "Bruxelles",
            "be",
            ProjectScenario.StartsOn,
            null,
            null,
            ProjectScenario.UserId
        );
}
