namespace HrAgencySystem.LegalEntities.Domain.ValueObjects;

/// <summary>
/// Whoever runs the entity - the person a register names as its president or managing director.
/// <para>
/// Deliberately not a <c>ContactPerson</c>: that one requires an e-mail address, and a president is
/// usually read off a public register that gives a name and nothing else. Demanding an address we
/// do not have would either block recording the entity or invite a made up one.
/// </para>
/// </summary>
public sealed record President(string FirstName, string LastName, string? Email)
{
    public string FullName => $"{FirstName} {LastName}".Trim();

    public static (President? president, List<string> errors) TryCreate(
        string? firstName,
        string? lastName,
        string? email
    )
    {
        var errors = new List<string>();

        var (first, firstError) = SharedKernel.ValueObjects.FirstName.TryCreate(firstName ?? "");
        var (last, lastError) = SharedKernel.ValueObjects.LastName.TryCreate(lastName ?? "");

        if (firstError is not null)
            errors.Add(firstError);

        if (lastError is not null)
            errors.Add(lastError);

        // The address is the one optional part, but a given one still has to be an address.
        string? normalizedEmail = null;

        if (!string.IsNullOrWhiteSpace(email))
        {
            var (parsed, emailError) = SharedKernel.ValueObjects.Email.TryCreate(email);

            if (emailError is not null)
                errors.Add(emailError);
            else
                normalizedEmail = parsed!.Value;
        }

        if (errors.Count > 0)
            return (null, errors);

        return (new President(first!.Value, last!.Value, normalizedEmail), []);
    }
}
