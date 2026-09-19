namespace HrAgencySystem.Teams.Domain;

public readonly record struct TeamId(Guid Value)
{
    public static TeamId New()
    {
        return new TeamId(Guid.NewGuid());
    }

    public static TeamId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Team ID cannot be empty.", nameof(value));

        return new TeamId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
