using System.Diagnostics.CodeAnalysis;
using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Agency.Application.TimeSheets;

/// <summary>
/// The refusals shared by more than one time sheet command, in one place so the same situation gets
/// the same sentence whichever way somebody arrives at it.
/// </summary>
public static class TimeSheetRules
{
    public const string UnknownTimeSheetMessage =
        "There is no time sheet for that person and month.";

    public const string NotEditableMessage =
        "This sheet is not open for editing. Only a draft or a sheet sent back for correction can be changed.";

    public const string NotCoveredMessage =
        "This person's contract carries no duty to record hours, so there is nothing to fill in.";

    public const string NoEmploymentMessage =
        "This person has no employment record, so there is no month to record hours against.";

    public const string DateOutsideMonthMessage =
        "That day does not belong to the month of this sheet.";

    public const string EmptySheetMessage =
        "A month with no hours on it cannot be sent for approval.";

    public const string NotTheSupervisorMessage =
        "Only the person this sheet answers to can decide it.";

    public const string NotPayrollMessage = "Only payroll settles an approved month.";

    public const string NoDayOnThatDateMessage = "Nothing is recorded on that day.";

    /// <summary>
    /// Whether the actor stands above the sheet's owner, worked out now rather than read off the
    /// sheet - see <see cref="SupervisorPolicy.IsAbove"/> for why the answer is not frozen.
    /// </summary>
    public static async Task EnsureIsSupervisorOf(
        IOrgStructureQueryRepository chart,
        OrganizationId organizationId,
        Guid actorId,
        Guid ownerId,
        CancellationToken ct
    )
    {
        var structure = await chart.GetStructureAsync(organizationId, ct);

        if (structure is null || !SupervisorPolicy.IsAbove(structure.AsUnits(), actorId, ownerId))
            throw new BusinessRuleException(NotTheSupervisorMessage);
    }

    /// <summary>
    /// A month nobody has written on has no stream, so the aggregate handler is handed nothing.
    /// That is a sheet that does not exist yet, not a broken call - it answers 404 and says so.
    /// </summary>
    public static void EnsureExists([NotNull] TimeSheet? sheet)
    {
        if (sheet is null)
            throw new NotFoundException(UnknownTimeSheetMessage);
    }

    public static void EnsureEditable(TimeSheet sheet)
    {
        if (!sheet.IsEditable)
            throw new BusinessRuleException(NotEditableMessage);
    }

    public static void EnsureCanChange(TimeSheetStatus from, TimeSheetStatus to)
    {
        if (!TimeSheetStatusChangePolicy.CanChange(from, to))
            throw new BusinessRuleException($"A sheet that is {from} cannot become {to}.");
    }
}
