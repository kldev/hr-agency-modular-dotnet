using HrAgencySystem.Projects.Application.Emails.Set;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.UnitTests.Projects;

public class ProjectEmailRecipientsHandlerTests : BaseTest
{
    [Fact]
    public async Task Set_KeepsSeveralAddressesForOnePurpose()
    {
        // A list from the first day: invoicing addresses come in twos and threes often enough that
        // turning a single field into a list later would be an API change for no reason.
        var (result, _) = await Set(
            ProjectScenario.Draft(),
            EmailPurpose.Invoice,
            ["ap@acme.example.com", "finance@acme.example.com"]
        );

        Assert.Equal(EmailPurpose.Invoice, result.Purpose);
        Assert.Equal(2, result.Emails.Count);
    }

    [Fact]
    public async Task Set_ReplacesOnlyItsOwnPurpose()
    {
        var project = ProjectScenario.Draft();

        var (invoices, _) = await Set(project, EmailPurpose.Invoice, ["ap@acme.example.com"]);
        project.Apply(invoices);

        var (documents, _) = await Set(project, EmailPurpose.Document, ["docs@acme.example.com"]);
        project.Apply(documents);

        var (replacement, _) = await Set(project, EmailPurpose.Invoice, ["billing@acme.example.com"]);
        project.Apply(replacement);

        Assert.Equal(2, project.EmailRecipients.Count);
        Assert.Contains(
            project.EmailRecipients,
            r => r.Purpose == EmailPurpose.Document && r.Email == "docs@acme.example.com"
        );
        Assert.Contains(
            project.EmailRecipients,
            r => r.Purpose == EmailPurpose.Invoice && r.Email == "billing@acme.example.com"
        );
    }

    [Fact]
    public async Task Set_AnEmptyListClearsThePurpose()
    {
        var project = ProjectScenario.Draft();

        var (invoices, _) = await Set(project, EmailPurpose.Invoice, ["ap@acme.example.com"]);
        project.Apply(invoices);

        var (cleared, _) = await Set(project, EmailPurpose.Invoice, []);
        project.Apply(cleared);

        Assert.Empty(project.EmailRecipients);
    }

    [Fact]
    public async Task Set_WithABadAddress_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Set(ProjectScenario.Draft(), EmailPurpose.Invoice, ["ap@acme.example.com", "nope"])
        );

        Assert.Contains(Email.InvalidEmail, error.Errors);
    }

    [Fact]
    public async Task Set_WithTheSameAddressTwice_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Set(
                ProjectScenario.Draft(),
                EmailPurpose.Invoice,
                ["ap@acme.example.com", "AP@ACME.EXAMPLE.COM"]
            )
        );

        Assert.Contains(SetProjectEmailRecipientsHandler.DuplicateEmailMessage, error.Errors);
    }

    private static Task<(
        HrAgencySystem.Projects.Events.ProjectEmailRecipientsChanged,
        Wolverine.Marten.Events
    )> Set(Project project, EmailPurpose purpose, IReadOnlyList<string> emails) =>
        SetProjectEmailRecipientsHandler.Handle(
            new SetProjectEmailRecipients(
                ProjectScenario.ProjectId,
                ProjectScenario.OrganizationId,
                purpose,
                emails,
                ProjectScenario.UserId
            ),
            project,
            ProjectScenario.Service(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );
}
