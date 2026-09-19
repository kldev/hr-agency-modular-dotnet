using HrAgencySystem.EmailTemplates.Contracts.Identity;
using HrAgencySystem.EmailTemplates.Contracts.Recruitment;
using HrAgencySystem.EmailTemplates.Contracts.Sales;

namespace HrAgencySystem.EmailTemplates.Rendering;

public interface IEmailTemplateProvider
{
    Task<string> RenderSendJobApplicationCreated(SendJobApplicationCreated data);

    Task<string> RenderSendPasswordReset(SendPasswordReset data);

    Task<string> RenderSendJobPostRecruiterChanged(SendJobPostRecruiterChanged data);

    Task<string> RenderSendOpportunityCreated(SendOpportunityCreated data);

    Task<string> RenderSendOpportunityResponsibleChanged(SendOpportunityResponsibleChanged data);
}
