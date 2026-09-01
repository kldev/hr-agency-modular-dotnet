using HrAgencySystem.Identity.Domain;

namespace HrAgencySystem.Identity.Application.Commands;

public sealed record LoginUser(string Email, string Password, string Slug);

public sealed record LoginUserResult(string Token);