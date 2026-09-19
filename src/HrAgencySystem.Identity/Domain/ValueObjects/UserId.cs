using HrAgencySystem.SharedKernel.Extensions;

namespace HrAgencySystem.Identity.Domain.ValueObjects;

public sealed record UserId
{
    private UserId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static UserId New() => new(Guid.NewGuid());

    public static UserId From(Guid value) => new(value);

    public static UserId? From(Guid? value) => value.IsInvalid() ? null : From(value!.Value);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(UserId id) => id.Value;

    public static implicit operator Guid?(UserId? id) => id?.Value;
}
