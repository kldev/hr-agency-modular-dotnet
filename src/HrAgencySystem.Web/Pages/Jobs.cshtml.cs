using HrAgencySystem.SharedKernel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HrAgencySystem.Web.Pages;

public class Jobs(IQueryOrganizationRepository repository) : PageModel
{
    public string OrganizationName { get; private set; } = "";
    
    public String Slug { get; private set; } = "";

    
    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
    {

        Slug = slug;
        var organization = await repository.GetBySlugAsync(slug, ct);

        if (organization == null)
        {
            return NotFound();
        }

        OrganizationName = organization.Name;
        
        return Page();
    }
}