namespace HrAgencySystem.Projects.Domain;

public readonly record struct ProjectId(Guid Value)
{
    public static ProjectId New()
    {
        return new ProjectId(Guid.NewGuid());
    }

    // Validates empty, unlike JobPostId and SalesOpportunityId. A new module has no old callers to
    // break, so it starts on the stricter side of a difference the codebase currently has both ways.
    public static ProjectId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Project ID cannot be empty.", nameof(value));

        return new ProjectId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
