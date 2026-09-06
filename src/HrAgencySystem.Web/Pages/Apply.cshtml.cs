using System.ComponentModel.DataAnnotations;
using HrAgencySystem.Recruitment.Application.JobApplication.Create;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Feeds.Serialization;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wolverine;

namespace HrAgencySystem.Web.Pages;

public partial class Apply(IMessageBus bus, IOrganizationService service, IJobPostQueryRepository jobPostQueryRepository, ILogger<Apply> logger) : PageModel
{
    public JobJson Job { get; private set; } = null!;
    public string Slug { get; set; } = "";
    
    public async Task<IActionResult> OnGetAsync(string slug, string postslug, CancellationToken ct)
    {
        var job = await GetJobAsync(slug, postslug, ct);

        if (job is null)
            return NotFound();

        Job = JobJson.FromProjection(job);
        Slug = slug;

        return Page();
    }
    
    [BindProperty]
    [Required(ErrorMessage = "First name is required.")]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Last name is required.")]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "E-mail address is required.")]
    [EmailAddress(ErrorMessage = "Provide a valid e-mail address.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;
    
    [BindProperty]
    [Display(Name = "Phone")]
    public string? Phone { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync(string slug,
        string postslug,
        CancellationToken ct)
    {
        LogApplyingJobSlugWithEmail(slug, Email);
        var job = await GetJobAsync(slug, postslug, ct);
        Slug = slug;
        if (job is null)
            return NotFound();

        Job = JobJson.FromProjection(job);

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var request = new ApplyToJobApplication(job.Id, Email, Phone ?? "", CandidateSource.CareerPage, FirstName, LastName);
            
            await bus.InvokeAsync<JobApplicationCreated>(request, ct);
            return Redirect($"/{slug}/success.html");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return RedirectToPage("/Error");
        }
        
        // todo:
        return Page();
    }
    
    private async Task<JobPostProjection?> GetJobAsync(
        string slug,
        string postslug,
        CancellationToken ct)
    {
        var organization = await service.GetBySlugAsync(slug, ct);

        if (organization is null)
            return null;

        return await jobPostQueryRepository.GetJobPost(
            organization.Id,
            $"{organization.Slug}/{postslug}",
            ct);
    }

    [LoggerMessage(LogLevel.Information, "Applying job {slug} with {email}")]
    partial void LogApplyingJobSlugWithEmail(string slug, string email);
}