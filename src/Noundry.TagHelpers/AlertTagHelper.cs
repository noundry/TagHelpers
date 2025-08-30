using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Noundry.TagHelpers;

/// <summary>
/// Creates Bootstrap alert components with various styling options and automatic dismissal.
/// </summary>
[HtmlTargetElement("alert")]
[HtmlTargetElement("*", Attributes = "alert-type")]
public class AlertTagHelper : TagHelper
{
    /// <summary>
    /// The alert type/variant (primary, secondary, success, danger, warning, info, light, dark).
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
    /// Icon to display before the alert content. Uses Bootstrap Icons classes.
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
        
        // Add Bootstrap alert classes
        var classes = new List<string> { "alert", $"alert-{Type}" };
        
        if (Dismissible)
        {
            classes.AddRange(new[] { "alert-dismissible", "fade", "show" });
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
            contentBuilder.Append($"<i class=\"{Icon} me-2\"></i>");
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
            contentBuilder.Append("<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"alert\" aria-label=\"Close\"></button>");
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