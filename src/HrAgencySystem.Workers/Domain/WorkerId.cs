namespace HrAgencySystem.Workers.Domain;

public readonly record struct WorkerId(Guid Value)
{
    public static WorkerId New() => new(Guid.NewGuid());

    public static WorkerId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Worker id cannot be empty.", nameof(value));

        return new WorkerId(value);
    }

    public override string ToString() => Value.ToString();
}
