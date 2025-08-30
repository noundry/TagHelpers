using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Noundry.TagHelpers;

/// <summary>
/// Creates Tailwind CSS alert components with various styling options and automatic dismissal.
/// </summary>
[HtmlTargetElement("alert")]
[HtmlTargetElement("*", Attributes = "alert-type")]
public class AlertTagHelper : TagHelper
{
    /// <summary>
    /// The alert type/variant (success, error, warning, info).
    /// </summary>
    [HtmlAttributeName("alert-type")]
    public string Type { get; set; } = "info";

    /// <summary>
    /// Whether the alert can be dismissed with a close button.
    /// </summary>
    [HtmlAttributeName("dismissible")]
    public bool Dismissible { get; set; } = false;

    /// <summary>
    /// Auto-dismiss the alert after specified milliseconds.
    /// </summary>
    [HtmlAttributeName("auto-dismiss")]
    public int? AutoDismiss { get; set; }

    /// <summary>
    /// Alert title/heading text.
    /// </summary>
    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Icon to display before the alert content. Uses Heroicons or custom icon classes.
    /// </summary>
    [HtmlAttributeName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the alert.
    /// </summary>
    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

    /// <summary>
    /// ARIA role for accessibility. Defaults to 'alert'.
    /// </summary>
    [HtmlAttributeName("role")]
    public string Role { get; set; } = "alert";

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        var childContent = await output.GetChildContentAsync();

        output.TagName = "div";
        output.Attributes.SetAttribute("role", Role);
        
        // Add Tailwind CSS alert classes
        var baseClasses = "relative px-4 py-3 rounded border";
        var typeClasses = Type.ToLower() switch
        {
            "success" => "bg-green-50 border-green-200 text-green-800",
            "error" or "danger" => "bg-red-50 border-red-200 text-red-800",
            "warning" => "bg-yellow-50 border-yellow-200 text-yellow-800",
            "info" => "bg-blue-50 border-blue-200 text-blue-800",
            _ => "bg-gray-50 border-gray-200 text-gray-800"
        };
        
        var classes = new List<string> { baseClasses, typeClasses };
        
        if (Dismissible)
        {
            classes.Add("pr-10"); // Add padding for close button
        }

        if (!string.IsNullOrEmpty(CssClass))
        {
            classes.Add(CssClass);
        }

        output.Attributes.SetAttribute("class", string.Join(" ", classes));

        var contentBuilder = new StringBuilder();

        // Add icon if specified
        if (!string.IsNullOrEmpty(Icon))
        {
            contentBuilder.Append($"<i class=\"{Icon} mr-2\"></i>");
        }

        // Add title if specified
        if (!string.IsNullOrEmpty(Title))
        {
            contentBuilder.Append($"<strong>{HtmlEncoder.Default.Encode(Title)}</strong>");
            if (!childContent.IsEmptyOrWhiteSpace)
            {
                contentBuilder.Append("<br>");
            }
        }

        // Add child content
        if (!childContent.IsEmptyOrWhiteSpace)
        {
            contentBuilder.Append(childContent.GetContent());
        }

        // Add dismiss button if dismissible
        if (Dismissible)
        {
            contentBuilder.Append("<button type=\"button\" class=\"absolute top-2 right-2 text-gray-400 hover:text-gray-600 focus:outline-none focus:text-gray-600\" onclick=\"this.parentElement.remove()\" aria-label=\"Close\">");
            contentBuilder.Append("<svg class=\"h-4 w-4\" fill=\"none\" viewBox=\"0 0 24 24\" stroke=\"currentColor\">");
            contentBuilder.Append("<path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M6 18L18 6M6 6l12 12\" />");
            contentBuilder.Append("</svg></button>");
        }

        output.Content.SetHtmlContent(contentBuilder.ToString());

        // Add auto-dismiss JavaScript if specified
        if (AutoDismiss.HasValue)
        {
            var script = $"<script>setTimeout(function() {{ document.querySelector('[role=\"{Role}\"]').remove(); }}, {AutoDismiss.Value});</script>";
            output.PostContent.SetHtmlContent(script);
        }
    }
}