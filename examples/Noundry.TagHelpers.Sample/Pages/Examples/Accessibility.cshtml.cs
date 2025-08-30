using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Noundry.TagHelpers.Sample.Pages.Examples;

public class AccessibilityModel : PageModel
{
    public int ProgressValue { get; set; } = 75;

    public void OnGet()
    {
        // Initialize accessibility examples
    }
}