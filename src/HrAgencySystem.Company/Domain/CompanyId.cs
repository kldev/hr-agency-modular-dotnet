namespace HrAgencySystem.Company.Domain;

public readonly record struct CompanyId(Guid Value)
{
    public static CompanyId New()
    {
        return new CompanyId(Guid.NewGuid());
    }

    public static CompanyId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException(
                "Company ID cannot be empty.",
                nameof(value));

        return new CompanyId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}