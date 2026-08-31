namespace HrAgencySystem.Identity.Domain.ValueObjects;

public sealed record UserOrganizationId
{
    private UserOrganizationId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static UserOrganizationId From(Guid value)
        => new(value);

    public override string ToString()
        => Value.ToString();
}