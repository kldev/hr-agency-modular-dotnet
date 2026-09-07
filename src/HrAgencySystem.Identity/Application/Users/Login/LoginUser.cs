namespace HrAgencySystem.Identity.Application.Users.Login;

public sealed record LoginUser(string Email, string Password, string Slug);

public sealed record LoginUserResult(string Token);