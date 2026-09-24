using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.Identity.Application.ApiKeys.Issue;

public static class IssueServiceApiKeyHandler
{
    public static ServiceApiKeyIssued Handle(
        IssueServiceApiKey command,
        IServiceApiKeyRepository keys,
        IClock clock
    )
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        var name = command.Name?.Trim() ?? "";

        if (name.Length == 0)
            throw new ValidationException(ServiceApiKey.NameRequiredMessage);

        if (name.Length > ServiceApiKey.NameMaxLength)
            throw new ValidationException(ServiceApiKey.NameTooLongMessage);

        var (key, value) = ServiceApiKey.Issue(name, command.IssuedBy, clock);

        keys.Issue(key);

        return new ServiceApiKeyIssued(key.Id, key.Name, value, key.CreatedAt);
    }
}
