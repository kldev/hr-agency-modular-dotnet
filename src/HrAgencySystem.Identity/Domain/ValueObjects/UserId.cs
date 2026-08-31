namespace HrAgencySystem.Identity.Domain.ValueObjects;

public sealed record UserId
{
    private UserId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static UserId New()
        => new(Guid.NewGuid());

    public static UserId From(Guid value)
        => new(value);

    public override string ToString()
        => Value.ToString();
}