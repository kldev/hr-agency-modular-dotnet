using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Agency.Application.Employment.Start;

public sealed record StartAgencyEmployment(
    Guid OrganizationId,
    Guid UserId,
    WorkerContractType ContractType,
    DateOnly StartsOn,
    decimal? WeeklyHours,
    RateInput? Rate,
    bool MayQuoteRate,
    Guid StartedBy
);

/// <summary>
/// A rate as it arrives from the outside, before anything vouches for it. Kept apart from
/// <see cref="WorkRate"/> so the value object stays the thing that has already been validated.
/// </summary>
public sealed record RateInput(decimal Amount, string Currency, RateUnit Unit, RateBasis Basis)
{
    /// <summary>
    /// Nothing quoted means no rate, which is allowed. Something quoted has to be a rate, and
    /// <c>WorkRate.TryCreate</c> is the one place that decides what that means.
    /// </summary>
    public static WorkRate? Validate(RateInput? input)
    {
        if (input is null)
            return null;

        var (rate, error) = WorkRate.TryCreate(
            input.Amount,
            input.Currency,
            input.Unit,
            input.Basis
        );

        return error is not null ? throw new ValidationException(error) : rate;
    }
}
