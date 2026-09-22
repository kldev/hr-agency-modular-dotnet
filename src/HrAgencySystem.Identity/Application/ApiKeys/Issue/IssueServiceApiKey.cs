namespace HrAgencySystem.Identity.Application.ApiKeys.Issue;

public sealed record IssueServiceApiKey(string Name, Guid IssuedBy);

/// <summary>
/// The only place the key's value ever appears. It is not stored and cannot be shown again - lose
/// it, and the answer is a new key.
/// </summary>
public sealed record ServiceApiKeyIssued(Guid Id, string Name, string Value, DateTimeOffset CreatedAt);
