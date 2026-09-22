namespace HrAgencySystem.PlatformSeeder;

public static class Config
{
    public const string TestPassword = "agent999!";

    /// <summary>
    /// The public job board's service key on a seeded platform. Fixed rather than random so the
    /// board can be configured before the API has ever run: the same value sits in
    /// <c>infrastructure/.env-sample</c> (<c>WebApiKey</c>) and in the board's
    /// <c>appsettings.Development.json</c>. Seed data only - never a key anybody should trust.
    /// </summary>
    public const string JobBoardApiKey = "sk_dev_public_job_board_seeded_key_not_a_secret";
}
