using System.ComponentModel.DataAnnotations;
using HrAgencySystem.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HrAgencySystem.Web.Pages;

public partial class Apply(IJobBoardClient board, ILogger<Apply> logger) : PageModel
{
    public JobView Job { get; private set; } = null!;
    public string Slug { get; set; } = "";

    public async Task<IActionResult> OnGetAsync(string slug, string postslug, CancellationToken ct)
    {
        BoardPost? post;

        try
        {
            post = await board.GetPostAsync(slug, postslug, ct);
        }
        catch (JobBoardUnavailableException)
        {
            return RedirectToPage("/Error");
        }

        if (post is null)
            return NotFound();

        Job = JobView.From(post);
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

    public async Task<IActionResult> OnPostAsync(string slug, string postslug, CancellationToken ct)
    {
        LogApplyingJobSlugWithEmail(slug, Email);
        Slug = slug;

        try
        {
            var post = await board.GetPostAsync(slug, postslug, ct);

            if (post is null)
                return NotFound();

            Job = JobView.From(post);

            if (!ModelState.IsValid)
                return Page();

            var result = await board.ApplyAsync(
                slug,
                postslug,
                new BoardApplication(FirstName, LastName, Email, Phone),
                ct
            );

            switch (result)
            {
                case ApplyResult.Accepted:
                    return Redirect($"/{slug}/success.html");

                case ApplyResult.PostNotFound:
                    return NotFound();

                // Said on the form, next to what the candidate typed, rather than on an error page
                // that throws the form away.
                case ApplyResult.Rejected rejected:
                    foreach (var reason in rejected.Reasons)
                        ModelState.AddModelError(string.Empty, reason);

                    return Page();
            }
        }
        catch (JobBoardUnavailableException exception)
        {
            logger.LogError(exception, "Applying to {Slug}/{PostSlug} failed", slug, postslug);
        }

        return RedirectToPage("/Error");
    }

    [LoggerMessage(LogLevel.Information, "Applying job {slug} with {email}")]
    partial void LogApplyingJobSlugWithEmail(string slug, string email);
}
