namespace HrAgencySystem.Recruitment.Domain.Applications;

public readonly record struct JobApplicationId(Guid Value)
{
    public static JobApplicationId New()
    {
        return new(Guid.NewGuid());
    }

    public static JobApplicationId From(Guid value)
    {
        return new(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}