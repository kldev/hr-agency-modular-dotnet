using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Agency.Domain.Employment;

/// <summary>
/// Who has to keep a record of their hours.
/// <para>
/// This is law, not configuration, which is why it is a function of the contract type and lives in
/// one place. A mandate contract has carried a statutory duty to record the number of hours worked
/// since the hourly minimum came in; an employment contract keeps working time by its own nature.
/// Somebody invoicing us bills a result, and there is nothing for us to record.
/// </para>
/// <para>
/// It matters beyond the person's own screen: without it the supervisor's monitoring cannot tell
/// "has not filled it in" from "does not have to".
/// </para>
/// </summary>
public static class TimeRecordPolicy
{
    public static bool RequiresTimeRecord(WorkerContractType contractType) =>
        contractType
            is WorkerContractType.EmploymentContract
                or WorkerContractType.TemporaryEmploymentContract
                or WorkerContractType.MandateContract;
}
