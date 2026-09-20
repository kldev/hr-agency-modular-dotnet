namespace HrAgencySystem.LegalEntities.Domain;

public readonly record struct LegalEntityId(Guid Value)
{
    public static LegalEntityId New()
    {
        return new LegalEntityId(Guid.NewGuid());
    }

    public static LegalEntityId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Legal entity ID cannot be empty.", nameof(value));

        return new LegalEntityId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
