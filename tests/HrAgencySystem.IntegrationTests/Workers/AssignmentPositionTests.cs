using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.IntegrationTests.Projects;
using HrAgencySystem.Workers.Domain;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Workers;

/// <summary>
/// The two facts that have to cross the module boundary: how many people a role holds, which is
/// counted where the role lives, and what the role is called, which is frozen where the posting
/// lives. Both travel as integration events, so both are only provable end to end.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class AssignmentPositionTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanWorkers();
        await Cleaner.CleanProjects();
        await Cleaner.CleanCompany();
        await Cleaner.CleanLegalEntities();
    }

    /// <summary>
    /// The seat is taken when somebody is planned onto the role and freed when the posting ends -
    /// which is what makes "this role is two people short" a question with an answer.
    /// </summary>
    [Fact]
    public async Task A_role_counts_the_people_held_against_it()
    {
        var organizationId = Guid.NewGuid();
        var delivery = await ProjectClient.CreateWithPositionAsync(organizationId);
        var workerId = await WorkerClient.EmployedAsync(organizationId);

        var planned = await WorkerClient.PlanAsync(
            organizationId,
            workerId,
            delivery.ProjectId,
            delivery.PositionId
        );

        await Eventually.AssertAsync(async () =>
            Assert.Equal(1, await AssignedCountAsync(organizationId, delivery))
        );

        await WorkerClient.ChangeAssignmentStatusAsync(
            organizationId,
            planned.AssignmentId,
            AssignmentStatus.DidNotStart
        );

        await Eventually.AssertAsync(async () =>
            Assert.Equal(0, await AssignedCountAsync(organizationId, delivery))
        );
    }

    /// <summary>
    /// The name on a posting is a frozen copy, but it is not a forgotten one: renaming the role in
    /// the project brings every posting held against it back into step. The id never moves.
    /// </summary>
    [Fact]
    public async Task Renaming_a_role_reaches_the_postings_held_against_it()
    {
        var organizationId = Guid.NewGuid();
        var delivery = await ProjectClient.CreateWithPositionAsync(organizationId);
        var workerId = await WorkerClient.EmployedAsync(organizationId);

        var planned = await WorkerClient.PlanAsync(
            organizationId,
            workerId,
            delivery.ProjectId,
            delivery.PositionId
        );

        await Eventually.AssertAsync(async () =>
        {
            var assignment = await WorkerClient.GetAssignmentAsync(
                organizationId,
                planned.AssignmentId
            );

            Assert.Equal("Painter", assignment?.PositionName);
        });

        await ProjectClient.RenamePositionAsync(
            organizationId,
            delivery.ProjectId,
            delivery.PositionId,
            "Painter Belgium"
        );

        await Eventually.AssertAsync(async () =>
        {
            var assignment = await WorkerClient.GetAssignmentAsync(
                organizationId,
                planned.AssignmentId
            );

            Assert.NotNull(assignment);
            Assert.Equal("Painter Belgium", assignment.PositionName);
            Assert.Equal(delivery.PositionId, assignment.PositionId);

            // The person's own row carries the same copy, so it has to have been reached too.
            var worker = await WorkerClient.GetAsync(organizationId, workerId);
            Assert.Contains(worker?.Assignments ?? [], a => a.PositionName == "Painter Belgium");
        });
    }

    private async Task<int?> AssignedCountAsync(
        Guid organizationId,
        ProjectTestClient.SeededDelivery delivery
    )
    {
        var positions = await ProjectClient.GetPositionsAsync(organizationId, delivery.ProjectId);

        return positions?.Content.FirstOrDefault(p => p.Id == delivery.PositionId)?.AssignedCount;
    }
}
