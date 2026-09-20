using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// Where the work is actually done and for how long. <paramref name="WorkCountry"/> is kept
/// explicitly rather than read off the address every time, because it is what selects the country's
/// formal requirements and that lookup should not depend on parsing an address.
/// <para>
/// This is the project's placement, not anybody's. Who works here, on what position and over which
/// part of this period is an <c>Assignment</c> in the workers module - one per person, many per
/// project.
/// </para>
/// </summary>
public sealed record Placement(
    PostalAddress WorkplaceAddress,
    string WorkCountry,
    DateOnly StartsOn,
    DateOnly? EndsOn
)
{
    public const string EndsBeforeStartMessage =
        "The end date cannot be earlier than the start date.";
}
