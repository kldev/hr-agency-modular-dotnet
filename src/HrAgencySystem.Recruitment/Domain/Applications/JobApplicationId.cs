namespace HrAgencySystem.Recruitment.Domain.Applications;

public readonly record struct JobApplicationId(Guid Value)
{
    public static JobApplicationId New()
    {
        return new JobApplicationId(Guid.NewGuid());
    }

    public static JobApplicationId From(Guid value)
    {
        return new JobApplicationId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}