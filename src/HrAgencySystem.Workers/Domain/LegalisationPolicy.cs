using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// Whether a person has to go through legalisation before they can work here, and it is one
/// question: does their citizenship carry free movement.
/// <para>
/// A Ukrainian or Belarusian national needs a residence title, a PESEL, a bank account and a work
/// permit before anybody can put them on a project; a Polish or any other EEA national needs none of
/// it. The difference is real and it is the reason a legalisation department exists - but it is a
/// fact about a list of countries, so it is a list of countries here and not a branch anywhere else.
/// </para>
/// <para>
/// Switzerland is on the list although it is not in the EEA: the free movement agreement puts its
/// nationals in the same position for this purpose.
/// </para>
/// </summary>
public static class LegalisationPolicy
{
    private static readonly HashSet<string> FreeMovement =
    [
        "AT",
        "BE",
        "BG",
        "CH",
        "CY",
        "CZ",
        "DE",
        "DK",
        "EE",
        "ES",
        "FI",
        "FR",
        "GR",
        "HR",
        "HU",
        "IE",
        "IS",
        "IT",
        "LI",
        "LT",
        "LU",
        "LV",
        "MT",
        "NL",
        "NO",
        "PL",
        "PT",
        "RO",
        "SE",
        "SI",
        "SK",
    ];

    public static bool RequiresLegalisation(string citizenship) =>
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        !FreeMovement.Contains((citizenship ?? "").Trim().ToUpperInvariant());

    public static bool RequiresLegalisation(CountryCode citizenship) =>
        RequiresLegalisation(citizenship.Value);
}
