namespace HrAgencySystem.Identity.Application.Owners.Login;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record LoginOwner(string Email, string Password);

public sealed record LoginOwnerResult(string Token);
