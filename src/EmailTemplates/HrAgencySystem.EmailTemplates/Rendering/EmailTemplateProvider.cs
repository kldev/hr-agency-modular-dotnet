using System.Reflection;
using FluentEmail.Liquid;
using HrAgencySystem.EmailTemplates.Contracts.Agency;
using HrAgencySystem.EmailTemplates.Contracts.Identity;
using HrAgencySystem.EmailTemplates.Contracts.Recruitment;
using HrAgencySystem.EmailTemplates.Contracts.Sales;
using HrAgencySystem.EmailTemplates.Contracts.Teams;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.EmailTemplates.Rendering;

public sealed class EmailTemplateProvider : IEmailTemplateProvider
{
    private const string NotifyRecruiterJobApplicationCreatedFile =
        "Mail/NotifyRecruiterJobApplicationCreated.liquid";

    private const string ResetPasswordFile = "Mail/ResetPassword.liquid";

    private const string NotifyRecruiterJobPostAssignedFile =
        "Mail/NotifyRecruiterJobPostAssigned.liquid";

    private const string NotifyOpportunityCreatedFile = "Mail/NotifyOpportunityCreated.liquid";

    private const string NotifyOpportunityResponsibleChangedFile =
        "Mail/NotifyOpportunityResponsibleChanged.liquid";

    private const string NotifyTeamMemberAddedFile = "Mail/NotifyTeamMemberAdded.liquid";

    private const string NotifyTeamMemberRoleChangedFile =
        "Mail/NotifyTeamMemberRoleChanged.liquid";

    private const string NotifyTimeSheetApprovedFile = "Mail/NotifyTimeSheetApproved.liquid";

    private const string NotifyTimeSheetReturnedForCorrectionFile =
        "Mail/NotifyTimeSheetReturnedForCorrection.liquid";

    private const string NotifyTimeSheetSettledFile = "Mail/NotifyTimeSheetSettled.liquid";

    private static readonly Assembly TemplateAssembly = typeof(IEmailTemplateProvider).Assembly;

    private readonly IFileProvider _fileProvider = new EmbeddedFileProvider(TemplateAssembly);

    private readonly LiquidRenderer _renderer;

    public EmailTemplateProvider()
    {
        // The file provider is what lets a template pull in partials with {% render %}.
        var options = new LiquidRendererOptions { FileProvider = _fileProvider };

        _renderer = new LiquidRenderer(Options.Create(options));
    }

    public async Task<string> RenderSendJobApplicationCreated(SendJobApplicationCreated data)
    {
        var template = await ReadTemplate(NotifyRecruiterJobApplicationCreatedFile);

        return await _renderer.ParseAsync(template, data);
    }

    public async Task<string> RenderSendPasswordReset(SendPasswordReset data)
    {
        var template = await ReadTemplate(ResetPasswordFile);

        return await _renderer.ParseAsync(template, data);
    }

    public async Task<string> RenderSendJobPostRecruiterChanged(SendJobPostRecruiterChanged data)
    {
        var template = await ReadTemplate(NotifyRecruiterJobPostAssignedFile);

        return await _renderer.ParseAsync(template, data);
    }

    public async Task<string> RenderSendOpportunityCreated(SendOpportunityCreated data)
    {
        var template = await ReadTemplate(NotifyOpportunityCreatedFile);

        return await _renderer.ParseAsync(template, data);
    }

    public async Task<string> RenderSendOpportunityResponsibleChanged(
        SendOpportunityResponsibleChanged data
    )
    {
        var template = await ReadTemplate(NotifyOpportunityResponsibleChangedFile);

        return await _renderer.ParseAsync(template, data);
    }

    public async Task<string> RenderSendTeamMemberAdded(SendTeamMemberAdded data)
    {
        var template = await ReadTemplate(NotifyTeamMemberAddedFile);

        return await _renderer.ParseAsync(template, data);
    }

    public async Task<string> RenderSendTeamMemberRoleChanged(SendTeamMemberRoleChanged data)
    {
        var template = await ReadTemplate(NotifyTeamMemberRoleChangedFile);

        return await _renderer.ParseAsync(template, data);
    }

    private async Task<string> ReadTemplate(string path)
    {
        var file = _fileProvider.GetFileInfo(path);

        if (!file.Exists)
        {
            throw new FileNotFoundException(
                $"Email template '{path}' is not embedded in {TemplateAssembly.GetName().Name}.",
                path
            );
        }

        await using var stream = file.CreateReadStream();
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync();
    }

    public async Task<string> RenderSendTimeSheetApproved(SendTimeSheetApproved data)
    {
        var template = await ReadTemplate(NotifyTimeSheetApprovedFile);

        return await _renderer.ParseAsync(template, data);
    }

    public async Task<string> RenderSendTimeSheetReturnedForCorrection(
        SendTimeSheetReturnedForCorrection data
    )
    {
        var template = await ReadTemplate(NotifyTimeSheetReturnedForCorrectionFile);

        return await _renderer.ParseAsync(template, data);
    }

    public async Task<string> RenderSendTimeSheetSettled(SendTimeSheetSettled data)
    {
        var template = await ReadTemplate(NotifyTimeSheetSettledFile);

        return await _renderer.ParseAsync(template, data);
    }
}
