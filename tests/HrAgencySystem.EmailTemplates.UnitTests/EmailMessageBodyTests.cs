using System.Net;
using HrAgencySystem.EmailTemplates.Contracts.Identity;
using HrAgencySystem.EmailTemplates.Contracts.Recruitment;
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
