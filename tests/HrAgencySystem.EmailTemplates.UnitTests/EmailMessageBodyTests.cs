using System.Net;
using HrAgencySystem.EmailTemplates.Contracts.Identity;
using HrAgencySystem.EmailTemplates.Contracts.Recruitment;
using HrAgencySystem.EmailTemplates.Contracts.Sales;
using HrAgencySystem.EmailTemplates.Rendering;
using Xunit.Abstractions;

namespace HrAgencySystem.EmailTemplates.UnitTests;

public class EmailMessageBodyTests(ITestOutputHelper output)
{
    private readonly EmailTemplateProvider _renderer = new();

    [Fact]
    public async Task RenderMailApplicationCreated()
    {
        var application = Application();

        var html = await _renderer.RenderSendJobApplicationCreated(application);

        await Save(nameof(RenderMailApplicationCreated), html);

        // Fluid html-encodes every value, so assert on the text a reader actually sees.
        var text = WebUtility.HtmlDecode(html);

        Assert.NotEmpty(html);
        Assert.Contains(application.RecruiterFullname, text);
        Assert.Contains(application.JobPostTitle, text);
        Assert.Contains(application.ApplicantEmail, text);
        Assert.Contains(application.ApplicantFullname, text);
        Assert.Contains(application.ApplicantPhone, text);
        Assert.Contains(application.ApplicationUrl, text);

        // the shared partials are pulled in
        Assert.Contains("HR Agency Portal", text);
        Assert.Contains("Recruitment workspace", text);

        AssertNoUnresolvedLiquid(html);
    }

    [Fact]
    public async Task RenderMailApplicationCreatedWithoutOptionalData()
    {
        var application = Application() with { ApplicantPhone = "", ApplicationUrl = "" };

        var html = await _renderer.RenderSendJobApplicationCreated(application);

        Assert.DoesNotContain("Phone", html);
        Assert.DoesNotContain("Review application", html);

        AssertNoUnresolvedLiquid(html);
    }

    [Fact]
    public async Task RenderMailPasswordReset()
    {
        var reset = new SendPasswordReset(
            Guid.NewGuid(),
            "identity-service",
            "Demo User",
            "demo@demo.com",
            "https://portal.hr-agency.test/password/reset?token=6f1c1b7a-3f5d-4f9a-9a6d-2f4a1c7c9b10",
            30
        );

        var html = await _renderer.RenderSendPasswordReset(reset);

        await Save(nameof(RenderMailPasswordReset), html);

        var text = WebUtility.HtmlDecode(html);

        Assert.NotEmpty(html);
        Assert.Contains(reset.RecipientFullname, text);
        Assert.Contains(reset.RecipientEmail, text);
        Assert.Contains(reset.ResetUrl, text);
        Assert.Contains($"{reset.ExpiresInMinutes} minutes", text);

        AssertNoUnresolvedLiquid(html);
    }

    [Fact]
    public async Task RenderMailEscapesCandidateData()
    {
        var application = Application() with
        {
            ApplicantFullname = "<script>alert('xss')</script>",
        };

        var html = await _renderer.RenderSendJobApplicationCreated(application);

        await Save(nameof(RenderMailEscapesCandidateData), html);

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("script", WebUtility.HtmlDecode(html));
    }

    [Fact]
    public async Task RenderMailJobPostRecruiterChanged()
    {
        var handover = new SendJobPostRecruiterChanged(
            Guid.NewGuid(),
            "recruitment-service",
            Guid.NewGuid(),
            "C# Developer",
            "katy.wells@hr-agency.test",
            "Katy Wells",
            "John Smith"
        );

        var html = await _renderer.RenderSendJobPostRecruiterChanged(handover);

        await Save(nameof(RenderMailJobPostRecruiterChanged), html);

        var text = WebUtility.HtmlDecode(html);

        Assert.Contains(handover.JobPostTitle, text);
        Assert.Contains(handover.RecruiterFullname, text);
        Assert.Contains(handover.ChangedByFullname, text);
        Assert.Contains(handover.JobPostId.ToString(), text);
        Assert.Contains("HR Agency Portal", text);

        AssertNoUnresolvedLiquid(html);
    }

    [Fact]
    public async Task RenderMailOpportunityCreated()
    {
        var created = new SendOpportunityCreated(
            Guid.NewGuid(),
            "sales-service",
            Guid.NewGuid(),
            "Contoso",
            Guid.NewGuid(),
            "Katy Wells",
            "katy.wells@hr-agency.test",
            "Team of four .NET engineers"
        );

        var html = await _renderer.RenderSendOpportunityCreated(created);

        await Save(nameof(RenderMailOpportunityCreated), html);

        var text = WebUtility.HtmlDecode(html);

        Assert.Contains(created.OpportunityTitle, text);
        Assert.Contains(created.CompanyName, text);
        Assert.Contains(created.ResponsiblePersonFullName, text);
        Assert.Contains(created.OpportunityId.ToString(), text);

        AssertNoUnresolvedLiquid(html);
    }

    [Fact]
    public async Task RenderMailOpportunityResponsibleChanged()
    {
        var handover = new SendOpportunityResponsibleChanged(
            Guid.NewGuid(),
            "sales-service",
            Guid.NewGuid(),
            "Team of four .NET engineers",
            "katy.wells@hr-agency.test",
            "Katy Wells",
            "Bob Smith",
            "John Smith"
        );

        var html = await _renderer.RenderSendOpportunityResponsibleChanged(handover);

        await Save(nameof(RenderMailOpportunityResponsibleChanged), html);

        var text = WebUtility.HtmlDecode(html);

        Assert.Contains(handover.OpportunityTitle, text);
        Assert.Contains(handover.ResponsibleFullname, text);
        Assert.Contains(handover.PreviousResponsibleFullname, text);
        Assert.Contains(handover.ChangedByFullname, text);
        Assert.Contains(handover.OpportunityId.ToString(), text);

        AssertNoUnresolvedLiquid(html);
    }

    private static SendJobApplicationCreated Application() =>
        new(
            Guid.NewGuid(),
            "recruitment-service",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "C# Developer",
            "demo@demo.com",
            "Demo User",
            "+48 600 100 200",
            "Test Recruiter",
            "test.recruiter@hr-agency.test",
            "https://portal.hr-agency.test/recruitment/job-applications/6f1c1b7a"
        );

    private static void AssertNoUnresolvedLiquid(string html)
    {
        Assert.DoesNotContain("{{", html);
        Assert.DoesNotContain("{%", html);
    }

    private async Task Save(string name, string html)
    {
        var savePath = Path.Combine(TestDirectoryHelper.GetTempDirectory(), name + ".html");

        output.WriteLine("Saving to {0}", savePath);

        await File.WriteAllTextAsync(savePath, html);
    }
}
