using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Sales.Domain.Opportunity.ValueObjects;

public sealed record OpportunityTitle
{
    private const int MaxLength = 300;
    
    private const string MaxLengthMessage =
        "Title cannot exceed 300 characters.";
    
    private const string RequiredMessage = "Title is required.";

    private OpportunityTitle(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static OpportunityTitle Create(string value)
    {
        var (title, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : title!;
    }

    public static (OpportunityTitle? title, string? error) TryCreate(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);
       

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new OpportunityTitle(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}