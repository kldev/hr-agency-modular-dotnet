using HrAgencySystem.SharedKernel.Extensions;

namespace HrAgencySystem.SharedKernel.Tenant;

public readonly record struct OrganizationId
{
    public const string OrganizationCheckMessage = "Non existing organization.";
    public const string OrganizationNotMatchMessage = "Organization not match.";

    public Guid Value { get; }

    private OrganizationId(Guid value)
    {
        Value = value.EnsureNotDefault(nameof(value));
    }

    public static OrganizationId From(Guid value) => new(value);

    public static OrganizationId? From(Guid? value) =>
        value.IsInvalid() ? null : From(value!.Value);

    public static OrganizationId NewId() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(OrganizationId id) => id.Value;

    public static implicit operator Guid?(OrganizationId? id) => id?.Value;
}

public static class OrganizationIdExtensions
{
    extension(OrganizationId? id)
    {
        public bool IsInvalid()
        {
            return !id.HasValue;
        }
    }
}
