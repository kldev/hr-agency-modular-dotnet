namespace HrAgencySystem.Identity.Domain.ValueObjects;

public readonly record struct PlatformOwnerId(Guid Value)
{
    public static PlatformOwnerId New() => new(Guid.NewGuid());

    public static PlatformOwnerId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(PlatformOwnerId id) => id.Value;
}
