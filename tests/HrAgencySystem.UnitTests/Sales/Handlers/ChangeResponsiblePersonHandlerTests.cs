using HrAgencySystem.EmailTemplates.Contracts.Sales;
using HrAgencySystem.Sales.Application.Opportunities.ChangeResponsible;
using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using NSubstitute;
using Wolverine;

namespace HrAgencySystem.UnitTests.Sales.Handlers;

public sealed class ChangeResponsiblePersonHandlerTests
{
    private readonly ISalesService _service = Substitute.For<ISalesService>();
    private readonly IClock _clock = Substitute.For<IClock>();

    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly Guid OpportunityId = Guid.NewGuid();

    private static readonly UserSnapshot Previous = new(
        Guid.NewGuid(),
        "Bob",
        "Smith",
        "bob.smith@hr-agency.com"
    );

    private static readonly UserSnapshot NewResponsible = new(
        Guid.NewGuid(),
        "Katy",
        "Wells",
        "katy.wells@hr-agency.com"
    );

    private static readonly UserSnapshot ChangedBy = new(
        Guid.NewGuid(),
        "John",
        "Smith",
        "john.smith@hr-agency.com"
    );

    [Fact]
    public async Task Handle_WithResponsibleOtherThanModifier_SendsNotification()
    {
        var (_, _, messages) = await Handle(NewResponsible, ChangedBy);

        var notification = Assert.Single(messages.OfType<SendOpportunityResponsibleChanged>());
        Assert.Equal(OpportunityId, notification.OpportunityId);
        Assert.Equal("Senior .NET Developer", notification.OpportunityTitle);
        Assert.Equal(NewResponsible.Email, notification.ResponsibleEmail);
        Assert.Equal(NewResponsible.Fullname, notification.ResponsibleFullname);
        Assert.Equal(Previous.Fullname, notification.PreviousResponsibleFullname);
        Assert.Equal(ChangedBy.Fullname, notification.ChangedByFullname);
    }

    [Fact]
    public async Task Handle_WithModifierTakingItOver_SendsNoNotification()
    {
        var (_, _, messages) = await Handle(ChangedBy, ChangedBy);

        Assert.Empty(messages);
    }

    private async Task<(
        ResponsiblePersonChanged,
        Wolverine.Marten.Events,
        OutgoingMessages
    )> Handle(UserSnapshot responsible, UserSnapshot modifiedBy)
    {
        var command = new ChangeResponsiblePerson(
            OpportunityId,
            OrganizationId,
            responsible.Id,
            modifiedBy.Id
        );

        _service.GetUserAsync(responsible.Id, Arg.Any<CancellationToken>()).Returns(responsible);
        _service.GetUserAsync(modifiedBy.Id, Arg.Any<CancellationToken>()).Returns(modifiedBy);
        _clock.UtcNow.Returns(new DateTimeOffset(2026, 9, 19, 10, 0, 0, TimeSpan.Zero));

        return await ChangeResponsiblePersonHandler.Handle(
            command,
            CreateAggregate(),
            _service,
            _clock,
            CancellationToken.None
        );
    }

    private static SalesOpportunity CreateAggregate()
    {
        var aggregate = SalesOpportunity.Empty();
        aggregate.Apply(
            new OpportunityCreated(
                OpportunityId,
                OrganizationId,
                new CompanySnapshot(Guid.NewGuid(), "Company A", "TX-100-101"),
                "Senior .NET Developer",
                "Potential software development opportunity.",
                OpportunityStage.New,
                50000m,
                CurrencyCode.PLN,
                false,
                new DateOnly(2026, 12, 31),
                Previous,
                new DateTimeOffset(2026, 9, 1, 10, 0, 0, TimeSpan.Zero),
                Previous
            )
        );

        return aggregate;
    }
}
