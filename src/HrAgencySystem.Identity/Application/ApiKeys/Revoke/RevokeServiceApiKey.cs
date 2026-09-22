namespace HrAgencySystem.Identity.Application.ApiKeys.Revoke;

public sealed record RevokeServiceApiKey(Guid Id, Guid RevokedBy);

public sealed record ServiceApiKeyRevoked(Guid Id, DateTimeOffset RevokedAt);
