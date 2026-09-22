using HrAgencySystem.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HrAgencySystem.Web.Pages;

public class Success(IJobBoardClient board) : PageModel
{
    public string Slug { get; private set; } = "";
    public string OrganizationName { get; private set; } = "";

    /// <summary>
    /// The application is already in by the time this page shows, so the agency's name is a nicety:
    /// if the API cannot say it, the page still thanks the candidate rather than failing.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
    {
        Slug = slug;

        try
        {
            OrganizationName = (await board.GetBoardAsync(slug, ct))?.Name ?? "";
        }
        catch (JobBoardUnavailableException)
        {
            OrganizationName = "";
        }

        return Page();
    }
}
