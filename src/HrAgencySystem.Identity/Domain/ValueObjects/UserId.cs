namespace HrAgencySystem.Identity.Domain.ValueObjects;

public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());

    public static UserId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(UserId id) => id.Value;
}
