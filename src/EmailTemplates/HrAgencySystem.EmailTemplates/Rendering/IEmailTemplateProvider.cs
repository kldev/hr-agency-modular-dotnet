using HrAgencySystem.EmailTemplates.Contracts.Agency;
using HrAgencySystem.EmailTemplates.Contracts.Identity;
using HrAgencySystem.EmailTemplates.Contracts.Recruitment;
using HrAgencySystem.EmailTemplates.Contracts.Sales;
using HrAgencySystem.EmailTemplates.Contracts.Teams;

namespace HrAgencySystem.EmailTemplates.Rendering;

public interface IEmailTemplateProvider
{
    Task<string> RenderSendJobApplicationCreated(SendJobApplicationCreated data);

    Task<string> RenderSendPasswordReset(SendPasswordReset data);

    Task<string> RenderSendJobPostRecruiterChanged(SendJobPostRecruiterChanged data);

    Task<string> RenderSendOpportunityCreated(SendOpportunityCreated data);

    Task<string> RenderSendOpportunityResponsibleChanged(SendOpportunityResponsibleChanged data);

    Task<string> RenderSendTeamMemberAdded(SendTeamMemberAdded data);

    Task<string> RenderSendTeamMemberRoleChanged(SendTeamMemberRoleChanged data);

    Task<string> RenderSendTimeSheetApproved(SendTimeSheetApproved data);

    Task<string> RenderSendTimeSheetReturnedForCorrection(SendTimeSheetReturnedForCorrection data);

    Task<string> RenderSendTimeSheetSettled(SendTimeSheetSettled data);
}
