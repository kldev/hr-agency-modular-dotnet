using HrAgencySystem.Identity.Events;
using HrAgencySystem.Recruitment.Application.Interviews.Schedule;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Events.Interviews;
using Marten;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

public sealed class InterviewsScenario(
    IMessageBus bus,
    IQuerySession session)
{
    private const int DaysBefore = 31;
    private const int DaysAfter = 31;

    public async Task SeedAsync(
        Guid organizationId,
        int count = 50)
    {
        if (count <= 0)
            return;

        var applications = await session.Query<JobApplicationCreated>()
            .Where(x => x.OrganizationId == organizationId)
            .ToListAsync();

        var users = await session.Query<UserCreated>()
            .Where(x => x.OrganizationId == organizationId)
            .ToListAsync();

        if (applications.Count == 0)
            throw new InvalidOperationException(
                $"No job applications found for organization {organizationId}.");

        if (users.Count == 0)
            throw new InvalidOperationException(
                $"No users found for organization {organizationId}.");

        for (var i = 0; i < count; i++)
        {
            var application = applications[
                Random.Shared.Next(applications.Count)];

            var user = users[
                Random.Shared.Next(users.Count)];

            await CreateInterview(application, user);
        }
    }

    private async Task CreateInterview(
        JobApplicationCreated application,
        UserCreated user)
    {
        var scheduleAt = RandomScheduleDate();

        var command = new ScheduleInterview(
            application.JobApplicationId,
            application.OrganizationId,
            scheduleAt,
            RandomEnum<InterviewFormat>(),
            RandomEnum<InterviewType>(),
            RandomNote(),
            user.UserId,
            user.UserId,
            "Europe/Warsaw");

        try
        {
            await bus.InvokeAsync<InterviewCreated>(command);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static DateTime RandomScheduleDate()
    {
        var today = DateTime.Today;

        var dayOffset = Random.Shared.Next(
            -DaysBefore,
            DaysAfter + 1);

        var hour = Random.Shared.Next(8, 18);
        var minute = Random.Shared.Next(0, 4) * 15;

        return today
            .AddDays(dayOffset)
            .AddHours(hour)
            .AddMinutes(minute);
    }

    private static TEnum RandomEnum<TEnum>()
        where TEnum : struct, Enum
    {
        var values = Enum.GetValues<TEnum>();

        return values[Random.Shared.Next(values.Length)];
    }

    private static string RandomNote()
    {
        return Random.Shared.Next(0, 4) switch
        {
            0 => "Initial technical interview",
            1 => "Candidate screening interview",
            2 => "Interview with hiring manager",
            _ => "Follow-up interview"
        };
    }
}