namespace HrAgencySystem.Identity.Domain.ValueObjects;

public sealed record PlatformOwnerId
{
    private PlatformOwnerId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static PlatformOwnerId New()
        => new(Guid.NewGuid());

    public static PlatformOwnerId From(Guid value)
        => new(value);

    public override string ToString()
        => Value.ToString();
}