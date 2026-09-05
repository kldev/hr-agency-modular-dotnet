namespace HrAgencySystem.SharedKernel.Port;

public interface ISeeder
{
    Task SeedAsync(CancellationToken ct);
}