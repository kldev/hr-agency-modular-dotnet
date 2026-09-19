using HrAgencySystem.EmailTemplates.Contracts.Identity;
using HrAgencySystem.EmailTemplates.Contracts.Recruitment;

namespace HrAgencySystem.EmailTemplates.Rendering;

public interface IEmailTemplateProvider
{
    Task<string> RenderSendJobApplicationCreated(SendJobApplicationCreated data);

    Task<string> RenderSendPasswordReset(SendPasswordReset data);
}
