using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Projects.Application.Positions;

/// <summary>What survived validation, ready to be put on the event.</summary>
internal sealed record ValidatedPositionData(
    string Name,
    string ContractName,
    string WorkDescription,
    IReadOnlyList<string> Duties,
    IReadOnlyList<string> RequiredQualifications,
    WorkRate? Rate,
    PostalAddress? WorkplaceAddress,
    decimal? WeeklyHours,
    TimeOnly? WorkStartsAt,
    string WorkSchedule,
    int? PayoutDay,
    string ProbationPeriod,
    string NoticePeriod,
    IReadOnlyList<string> Allowances,
    int? PlannedHeadcount
);

internal static class PositionDataFactory
{
    public const string RateCurrencyRequiredMessage = "A rate needs the currency it is quoted in.";

    public const string WeeklyHoursOutOfRangeMessage =
        "Weekly hours have to be greater than zero and cannot exceed 168.";

    public const string PayoutDayOutOfRangeMessage = "The payout day has to be between 1 and 31.";

    public const string PlannedHeadcountOutOfRangeMessage =
        "The planned headcount has to be at least one.";

    private const int HoursInAWeek = 168;

    /// <summary>
    /// Collects every problem into one <see cref="ValidationException"/>, the rhythm the other
    /// factories here use - somebody filling in seventeen fields should learn about all of them at
    /// once, not one round trip at a time.
    /// </summary>
    public static ValidatedPositionData Create(IPositionData data)
    {
        var errors = new List<string>();

        var (name, nameError) = PersonJobTitle.TryCreate(data.Name, true);
        if (nameError is not null)
            errors.Add(nameError);

        // An empty contract name falls back to the internal one: most roles are called the same
        // thing on both sides, and the distinction only earns its keep when somebody needs it.
        var contractNameInput = string.IsNullOrWhiteSpace(data.ContractName)
            ? data.Name
            : data.ContractName;

        var (contractName, contractNameError) = PersonJobTitle.TryCreate(contractNameInput, true);
        if (contractNameError is not null)
            errors.Add(contractNameError);

        var (workDescription, workDescriptionError) = LongText.TryCreate(
            data.WorkDescription ?? ""
        );
        if (workDescriptionError is not null)
            errors.Add(workDescriptionError);

        var duties = ReadEntries(data.Duties, errors);
        var qualifications = ReadEntries(data.RequiredQualifications, errors);
        var allowances = ReadEntries(data.Allowances, errors);

        var rate = ReadRate(data, errors);
        var address = ReadAddress(data, errors);

        if (data.WeeklyHours is not null and (<= 0 or > HoursInAWeek))
            errors.Add(WeeklyHoursOutOfRangeMessage);

        if (data.PayoutDay is not null and (< 1 or > 31))
            errors.Add(PayoutDayOutOfRangeMessage);

        if (data.PlannedHeadcount is not null and < 1)
            errors.Add(PlannedHeadcountOutOfRangeMessage);

        var schedule = ReadNote(data.WorkSchedule, errors);
        var probation = ReadNote(data.ProbationPeriod, errors);
        var notice = ReadNote(data.NoticePeriod, errors);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new ValidatedPositionData(
            name!.Value,
            contractName!.Value,
            workDescription!.Value,
            duties,
            qualifications,
            rate,
            address,
            data.WeeklyHours,
            data.WorkStartsAt,
            schedule,
            data.PayoutDay,
            probation,
            notice,
            allowances,
            data.PlannedHeadcount
        );
    }

    /// <summary>Blank lines are dropped rather than rejected - a list editor leaves them behind.</summary>
    private static IReadOnlyList<string> ReadEntries(
        IReadOnlyList<string>? entries,
        List<string> errors
    )
    {
        if (entries is null)
            return [];

        var result = new List<string>();

        foreach (var entry in entries.Where(e => !string.IsNullOrWhiteSpace(e)))
        {
            var (text, error) = EntryText.TryCreate(entry);

            if (error is not null)
                errors.Add(error);
            else
                result.Add(text!.Value);
        }

        return result;
    }

    private static WorkRate? ReadRate(IPositionData data, List<string> errors)
    {
        if (data.RateAmount is null)
            return null;

        if (string.IsNullOrWhiteSpace(data.RateCurrency))
        {
            errors.Add(RateCurrencyRequiredMessage);
            return null;
        }

        var (rate, error) = WorkRate.TryCreate(
            data.RateAmount.Value,
            data.RateCurrency,
            data.RateUnit,
            data.RateBasis
        );

        if (error is not null)
            errors.Add(error);

        return rate;
    }

    /// <summary>
    /// All or nothing, and nothing is the normal answer: a position without its own address works
    /// where the project works. Touch one part of it and the whole address becomes required, so
    /// nobody ends up with a street and no city on a contract.
    /// </summary>
    private static PostalAddress? ReadAddress(IPositionData data, List<string> errors)
    {
        string[] parts =
        [
            data.Street ?? "",
            data.BuildingNumber ?? "",
            data.UnitNumber ?? "",
            data.PostalCode ?? "",
            data.City ?? "",
            data.CountryCode ?? "",
        ];

        if (parts.All(string.IsNullOrWhiteSpace))
            return null;

        var (address, addressErrors) = PostalAddress.TryCreate(
            data.Street ?? "",
            data.BuildingNumber ?? "",
            data.UnitNumber,
            data.PostalCode ?? "",
            data.City ?? "",
            data.CountryCode ?? ""
        );

        errors.AddRange(addressErrors);

        return address;
    }

    private static string ReadNote(string? value, List<string> errors)
    {
        var (note, error) = ShortNote.TryCreate(value ?? "", false);

        if (error is not null)
            errors.Add(error);

        return note?.Value ?? "";
    }
}
