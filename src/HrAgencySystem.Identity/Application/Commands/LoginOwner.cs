namespace HrAgencySystem.Identity.Application.Commands;

public sealed record LoginOwner(string Email, string Password);

public sealed record LoginOwnerResult(string Token);
