using HrAgencySystem.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HrAgencySystem.Web.Pages;

public class Jobs(IJobBoardClient board) : PageModel
{
    public string OrganizationName { get; private set; } = "";

    public string Slug { get; private set; } = "";

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
    {
        Slug = slug;

        BoardInfo? agency;

        try
        {
            agency = await board.GetBoardAsync(slug, ct);
        }
        catch (JobBoardUnavailableException)
        {
            return RedirectToPage("/Error");
        }

        if (agency is null)
            return NotFound();

        OrganizationName = agency.Name;

        return Page();
    }
}
