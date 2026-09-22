using HrAgencySystem.Agency.Application.Employment.Start;
using HrAgencySystem.Agency.Application.TimeSheets.Approve;
using HrAgencySystem.Agency.Application.TimeSheets.Return;
using HrAgencySystem.Agency.Application.TimeSheets.SaveWorkDay;
using HrAgencySystem.Agency.Application.TimeSheets.Settle;
using HrAgencySystem.Agency.Application.TimeSheets.Submit;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

/// <summary>
/// Employment records for everybody on the chart, and two months of hours on top of them.
/// <para>
/// The contract types are mixed on purpose. A register where everybody is on the same contract
/// shows nothing: the whole reason <c>AgencyEmployment</c> exists is to tell somebody who has not
/// filled their hours in from somebody who never has to, and that difference is invisible until
/// some of the staff are on contracts that carry no duty at all.
/// </para>
/// <para>
/// The months are left in different states for the same reason - a queue of one status is not a
/// queue, and neither the monitoring screen nor the approval list says anything on data where
/// everybody has done the same thing.
/// </para>
/// </summary>
internal sealed class TimeRecordScenario(IMessageBus bus, IDocumentSession session)
{
    /// <summary>
    /// Every fourth person invoices us and every fifth is on a contract for specific work, so
    /// roughly half the register is covered by the duty and half is not.
    /// </summary>
    private static WorkerContractType ContractFor(int index) =>
        (index % 5) switch
        {
            0 => WorkerContractType.EmploymentContract,
            1 => WorkerContractType.MandateContract,
            2 => WorkerContractType.MandateContract,
            3 => WorkerContractType.SelfEmployed,
            _ => WorkerContractType.Other,
        };

    internal async Task<int> Seed(Guid organizationId, Func<Task> waitForProjections)
    {
        var structure = await session.LoadAsync<OrgStructureProjection>(
            OrgStructureId.For(organizationId)
        );

        if (structure is null)
            return 0;

        var units = structure.AsUnits();

        // Everybody who sits somewhere in the chart. Somebody with no unit has no supervisor, so
        // their month would have nobody to approve it.
        var people = units
            .SelectMany(unit => unit.Members.Select(member => member.UserId))
            .Distinct()
            .ToList();

        var index = 0;
        var covered = new List<Guid>();

        foreach (var userId in people)
        {
            var contract = ContractFor(index);

            await bus.InvokeAsync<AgencyEmploymentStarted>(
                new StartAgencyEmployment(
                    organizationId,
                    userId,
                    contract,
                    DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1),
                    contract == WorkerContractType.EmploymentContract ? 40m : null,
                    userId
                )
            );

            if (
                contract
                is WorkerContractType.EmploymentContract
                    or WorkerContractType.MandateContract
            )
                covered.Add(userId);

            index++;
        }

        await waitForProjections();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var previous = today.AddMonths(-1);

        // Only the months that were actually written on can be walked through the flow - the rest
        // have no sheet at all, and asking to send one is a 404 rather than a state to seed.
        var withSheets = new List<Guid>();

        for (var i = 0; i < covered.Count; i++)
        {
            var userId = covered[i];

            // The last two are left with nothing at all - they are the rows the monitoring screen
            // exists for, and they only appear because their contract says they owe hours.
            if (i >= covered.Count - 2)
                continue;

            await FillMonth(organizationId, userId, previous, workedDays: 18);
            withSheets.Add(userId);

            if (i % 3 != 2)
                await FillMonth(organizationId, userId, today, workedDays: Math.Min(today.Day, 10));
        }

        await waitForProjections();

        await CloseLastMonth(organizationId, units, withSheets, previous);

        return withSheets.Count;
    }

    private async Task FillMonth(Guid organizationId, Guid userId, DateOnly month, int workedDays)
    {
        var day = new DateOnly(month.Year, month.Month, 1);
        var lastDay = day.AddMonths(1).AddDays(-1);
        var written = 0;

        while (day <= lastDay && written < workedDays)
        {
            if (day.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            {
                await bus.InvokeAsync<WorkDaySaved>(
                    new SaveWorkDay(
                        organizationId,
                        userId,
                        month.Year,
                        month.Month,
                        day,
                        new TimeOnly(8, 0),
                        // Minutes stay on the five-minute step the value object insists on.
                        written % 4
                        == 0
                            ? 7
                            : 8,
                        written % 3 == 0 ? 30 : 0,
                        written % 7 == 0 ? "Worked from the client's office." : null,
                        userId
                    )
                );

                written++;
            }

            day = day.AddDays(1);
        }
    }

    /// <summary>
    /// Walks last month through the flow so every state is on the data: some sheets still waiting,
    /// some approved, some already handed to payroll, one sent back.
    /// </summary>
    private async Task CloseLastMonth(
        Guid organizationId,
        IReadOnlyList<HrAgencySystem.Agency.Domain.OrgUnit> units,
        IReadOnlyList<Guid> withSheets,
        DateOnly month
    )
    {
        var step = 0;

        foreach (var userId in withSheets)
        {
            var supervisor = HrAgencySystem.Agency.Domain.SupervisorPolicy.SupervisorOf(
                units,
                userId
            );

            if (supervisor is null)
                continue;

            var submitted = await Submit(organizationId, userId, month);

            if (!submitted)
                continue;

            // Left in Submitted: without a few of these the approval queue is empty on fresh data.
            if (step % 4 == 0)
            {
                step++;
                continue;
            }

            if (step % 4 == 1)
            {
                await bus.InvokeAsync<TimeSheetReturnedForCorrection>(
                    new ReturnTimeSheetForCorrection(
                        organizationId,
                        userId,
                        month.Year,
                        month.Month,
                        "The last week is missing - please add it and send it again.",
                        false,
                        supervisor.Value
                    )
                );

                step++;
                continue;
            }

            await bus.InvokeAsync<TimeSheetApproved>(
                new ApproveTimeSheet(
                    organizationId,
                    userId,
                    month.Year,
                    month.Month,
                    null,
                    supervisor.Value
                )
            );

            if (step % 4 == 3)
                await bus.InvokeAsync<TimeSheetSettled>(
                    new SettleTimeSheet(
                        organizationId,
                        userId,
                        month.Year,
                        month.Month,
                        true,
                        supervisor.Value
                    )
                );

            step++;
        }
    }

    private async Task<bool> Submit(Guid organizationId, Guid userId, DateOnly month)
    {
        try
        {
            await bus.InvokeAsync<TimeSheetSubmitted>(
                new SubmitTimeSheet(organizationId, userId, month.Year, month.Month, userId)
            );

            return true;
        }
        catch (SharedKernel.Exception.BusinessRuleException)
        {
            // A month nobody wrote anything on cannot be sent, and that is a state worth leaving in
            // the data rather than forcing.
            return false;
        }
        catch (SharedKernel.Exception.NotFoundException)
        {
            return false;
        }
    }
}
