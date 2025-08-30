using System.Text;
using Microsoft.AspNetCore.Html;

namespace Noundry.TagHelpers;

/// <summary>
/// Automatically hides HTML elements after a specified delay with optional debug logging.
/// </summary>
[HtmlTargetElement("*", Attributes = "auto-hide")]
[HtmlTargetElement("auto-hide")]
public class AutoHideTagHelper : TagHelper
{
    /// <summary>
    /// Whether to enable auto-hide functionality.
    /// </summary>
    [HtmlAttributeName("auto-hide")]
    public bool AutoHide { get; set; } = true;

    /// <summary>
    /// Delay in seconds before hiding the element. Defaults to 5 seconds.
    /// </summary>
    [HtmlAttributeName("auto-hide-delay")]
    public int DelayInSeconds { get; set; } = 5;

    /// <summary>
    /// Whether to automatically start the hide timer after the element is rendered. Defaults to true.
    /// </summary>
    [HtmlAttributeName("auto-hide-start")]
    public bool AutoStart { get; set; } = true;

    /// <summary>
    /// Whether to enable debug logging to the browser console. Defaults to false.
    /// </summary>
    [HtmlAttributeName("auto-hide-debug")]
    public bool Debug { get; set; } = false;

    /// <summary>
    /// The CSS transition effect to use when hiding. Examples: 'fade', 'slide-up', 'scale'. Defaults to 'fade'.
    /// </summary>
    [HtmlAttributeName("auto-hide-effect")]
    public string Effect { get; set; } = "fade";

    /// <summary>
    /// Duration of the hide animation in milliseconds. Defaults to 300ms.
    /// </summary>
    [HtmlAttributeName("auto-hide-duration")]
    public int AnimationDuration { get; set; } = 300;

    /// <summary>
    /// Whether to remove the element from DOM after hiding (vs just hiding with display:none). Defaults to false.
    /// </summary>
    [HtmlAttributeName("auto-hide-remove")]
    public bool RemoveFromDom { get; set; } = false;

    /// <summary>
    /// CSS selector for elements that should pause the auto-hide timer when hovered. Defaults to current element.
    /// </summary>
    [HtmlAttributeName("auto-hide-pause-on")]
    public string? PauseOnHover { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        if (!AutoHide)
        {
            return;
        }

        // Ensure element has an ID for JavaScript targeting
        var elementId = output.Attributes["id"]?.Value?.ToString();
        if (string.IsNullOrEmpty(elementId))
        {
            elementId = $"auto-hide-{Guid.NewGuid():N}";
            output.Attributes.SetAttribute("id", elementId);
        }

        // If this is an auto-hide element (not an attribute), set default tag and styling
        if (output.TagName == "auto-hide")
        {
            output.TagName = "div";
            
            // Add default Tailwind classes if none specified
            if (!output.Attributes.ContainsName("class"))
            {
                output.Attributes.SetAttribute("class", "p-4 rounded-lg border");
            }
        }

        var childContent = await output.GetChildContentAsync();
        
        // Generate JavaScript for auto-hide functionality
        var script = GenerateAutoHideScript(elementId, DelayInSeconds, AutoStart, Debug, Effect, AnimationDuration, RemoveFromDom, PauseOnHover);
        
        // Add the script after the element content
        output.PostContent.AppendHtml($"<script>{script}</script>");
    }

    private string GenerateAutoHideScript(string elementId, int delay, bool autoStart, bool debug, string effect, int duration, bool remove, string? pauseOn)
    {
        var script = new StringBuilder();
        
        script.AppendLine("(function() {");
        script.AppendLine($"    const element = document.getElementById('{elementId}');");
        script.AppendLine("    if (!element) return;");
        script.AppendLine();
        
        if (debug)
        {
            script.AppendLine("    const log = (msg) => console.log(`[AutoHide:{elementId}] ${msg}`);");
        }
        else
        {
            script.AppendLine("    const log = () => {};");
        }
        
        script.AppendLine();
        script.AppendLine("    let hideTimer = null;");
        script.AppendLine("    let isPaused = false;");
        script.AppendLine();
        
        // Add CSS for animations
        script.AppendLine("    // Add transition styles");
        script.AppendLine($"    element.style.transition = 'all {duration}ms ease-in-out';");
        script.AppendLine();
        
        // Hide function with effects
        script.AppendLine("    const hideElement = () => {");
        script.AppendLine("        log('Hiding element');");
        
        switch (effect.ToLower())
        {
            case "fade":
                script.AppendLine("        element.style.opacity = '0';");
                break;
            case "slide-up":
                script.AppendLine("        element.style.transform = 'translateY(-100%)';");
                script.AppendLine("        element.style.opacity = '0';");
                break;
            case "scale":
                script.AppendLine("        element.style.transform = 'scale(0)';");
                script.AppendLine("        element.style.opacity = '0';");
                break;
            default:
                script.AppendLine("        element.style.opacity = '0';");
                break;
        }
        
        if (remove)
        {
            script.AppendLine($"        setTimeout(() => element.remove(), {duration});");
        }
        else
        {
            script.AppendLine($"        setTimeout(() => element.style.display = 'none', {duration});");
        }
        
        script.AppendLine("    };");
        script.AppendLine();
        
        // Start timer function
        script.AppendLine("    const startTimer = () => {");
        script.AppendLine("        if (hideTimer) clearTimeout(hideTimer);");
        script.AppendLine($"        log('Starting hide timer: {delay}s');");
        script.AppendLine($"        hideTimer = setTimeout(hideElement, {delay * 1000});");
        script.AppendLine("    };");
        script.AppendLine();
        
        // Pause/resume functionality
        if (!string.IsNullOrEmpty(pauseOn) || pauseOn == null)
        {
            var hoverTarget = string.IsNullOrEmpty(pauseOn) ? "element" : $"document.querySelector('{pauseOn}')";
            
            script.AppendLine($"    const hoverTarget = {hoverTarget};");
            script.AppendLine("    if (hoverTarget) {");
            script.AppendLine("        hoverTarget.addEventListener('mouseenter', () => {");
            script.AppendLine("            log('Pausing timer (hover)');");
            script.AppendLine("            if (hideTimer) clearTimeout(hideTimer);");
            script.AppendLine("            isPaused = true;");
            script.AppendLine("        });");
            script.AppendLine();
            script.AppendLine("        hoverTarget.addEventListener('mouseleave', () => {");
            script.AppendLine("            log('Resuming timer');");
            script.AppendLine("            isPaused = false;");
            script.AppendLine("            startTimer();");
            script.AppendLine("        });");
            script.AppendLine("    }");
            script.AppendLine();
        }
        
        // Expose control methods globally
        script.AppendLine($"    window.autoHide_{elementId.Replace("-", "_")} = {{");
        script.AppendLine("        start: startTimer,");
        script.AppendLine("        hide: hideElement,");
        script.AppendLine("        cancel: () => {");
        script.AppendLine("            log('Timer cancelled');");
        script.AppendLine("            if (hideTimer) clearTimeout(hideTimer);");
        script.AppendLine("        }");
        script.AppendLine("    };");
        script.AppendLine();
        
        if (autoStart)
        {
            script.AppendLine("    // Auto-start");
            script.AppendLine("    log('Auto-starting hide timer');");
            script.AppendLine("    startTimer();");
        }
        
        script.AppendLine("})();");
        
        return script.ToString();
    }
}