using HrAgencySystem.Compliance;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.Reports.ReadModel;
using HrAgencySystem.SharedKernel.Snapshots;
using JasperFx.Events;

namespace HrAgencySystem.UnitTests.Reports;

public sealed class ProjectReportProjectionTests
{
    private static readonly Guid ProjectId = Guid.NewGuid();
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly UserSnapshot Author = new(Guid.NewGuid(), "F", "L", "fl@test.pl");
    private static readonly DateTimeOffset Day1 = new(2026, 3, 2, 9, 0, 0, TimeSpan.Zero);

    private readonly ProjectReportProjection _projection = new();

    [Fact]
    public void GoingLive_KeepsTheFirstTimeThroughASuspension()
    {
        var row = Project(
            Created(),
            StatusChanged(ProjectStatus.Draft, ProjectStatus.Active, Day1.AddDays(10)),
            StatusChanged(ProjectStatus.Active, ProjectStatus.Suspended, Day1.AddDays(20)),
            StatusChanged(ProjectStatus.Suspended, ProjectStatus.Active, Day1.AddDays(30))
        );

        Assert.NotNull(row);
        Assert.Equal(OrganizationId, row.OrganizationId);
        Assert.Equal(nameof(EngagementType.Outsourcing), row.EngagementType);
        Assert.Equal("PL", row.CountryCode);
        Assert.Equal(nameof(ProjectStatus.Active), row.Status);
        Assert.Equal(Day1.AddDays(10), row.WentLiveAt);
        Assert.Equal(Day1.AddDays(30), row.UpdatedAt);
    }

    [Fact]
    public void Created_IsADraftThatNeverWentLive()
    {
        var row = Project(Created());

        Assert.Equal(nameof(ProjectStatus.Draft), row!.Status);
        Assert.Null(row.WentLiveAt);
        Assert.Equal(Day1, row.CreatedAt);
    }

    private ProjectReportRow? Project(params object[] events)
    {
        ProjectReportRow? row = null;

        foreach (var data in events)
        {
            var type = typeof(Event<>).MakeGenericType(data.GetType());
            var @event = (IEvent)Activator.CreateInstance(type, data)!;
            @event.Timestamp = Day1;

            row = _projection.ApplyEvent(row, ProjectId, @event, null!, null!);
        }

        return row;
    }

    private static ProjectCreated Created() =>
        new(
            ProjectId,
            OrganizationId,
            new CompanySnapshot(Guid.NewGuid(), "ACME", "5252344078"),
            null!,
            "Senior Developers Recruitment",
            "Description",
            EngagementType.Outsourcing,
            new Placement(null!, "PL", new DateOnly(2026, 10, 1), null),
            null,
            null,
            Author,
            Day1
        );

    private static ProjectStatusChanged StatusChanged(
        ProjectStatus from,
        ProjectStatus to,
        DateTimeOffset at
    ) => new(ProjectId, OrganizationId, from, to, "", Author, at);
}
