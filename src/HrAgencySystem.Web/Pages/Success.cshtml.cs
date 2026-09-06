using HrAgencySystem.SharedKernel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HrAgencySystem.Web.Pages;

public class Success(IOrganizationService service) : PageModel
{
    public string Slug { get; private set; } = "";
    public string OrganizationName { get; private set; } = "";
    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
    {
        
        Slug = slug;
        var organization = await service.GetBySlugAsync(slug, ct);

        if (organization == null)
        {
            OrganizationName = "";
            return Page();
        }

        OrganizationName = organization.Name;
        
        return Page();
    }
}