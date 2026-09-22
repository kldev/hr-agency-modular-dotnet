using HrAgencySystem.Agency.Domain.Employment;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// Who owes a record of their hours. This is law rather than configuration, and getting it wrong in
/// either direction is visible: too wide and the monitoring list fills with people who will never
/// type anything, too narrow and somebody's statutory record is simply missing.
/// </summary>
public class TimeRecordPolicyTests : BaseTest
{
    /// <summary>
    /// A mandate contract has carried the duty since the hourly minimum came in, and employment
    /// keeps working time by its own nature.
    /// </summary>
    [Theory]
    [InlineData(WorkerContractType.MandateContract)]
    [InlineData(WorkerContractType.EmploymentContract)]
    [InlineData(WorkerContractType.TemporaryEmploymentContract)]
    public void ContractsThatCarryTheDuty(WorkerContractType contract) =>
        Assert.True(TimeRecordPolicy.RequiresTimeRecord(contract));

    /// <summary>Somebody invoicing us bills a result. There is nothing for us to record.</summary>
    [Theory]
    [InlineData(WorkerContractType.SelfEmployed)]
    [InlineData(WorkerContractType.Other)]
    public void ContractsThatDoNot(WorkerContractType contract) =>
        Assert.False(TimeRecordPolicy.RequiresTimeRecord(contract));
}
