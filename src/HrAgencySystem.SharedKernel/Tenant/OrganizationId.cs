namespace HrAgencySystem.SharedKernel.Tenant;

public readonly record struct OrganizationId(Guid Value)
{
    public static OrganizationId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Organization ID cannot be empty.",
                nameof(value));

        return new OrganizationId(value);
    }

    public static OrganizationId NewId()
    {
        return new OrganizationId(Guid.NewGuid());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}