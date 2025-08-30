using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Noundry.TagHelpers;

/// <summary>
/// Renders validation messages for model properties with Tailwind CSS styling.
/// </summary>
[HtmlTargetElement("validation-message")]
[HtmlTargetElement("*", Attributes = "asp-validation-for")]
public class ValidationMessageTagHelper : TagHelper
{
    private readonly IHtmlHelper _htmlHelper;

    /// <summary>
    /// Creates a new instance of the <see cref="ValidationMessageTagHelper" /> class.
    /// </summary>
    /// <param name="htmlHelper">The <see cref="IHtmlHelper"/>.</param>
    public ValidationMessageTagHelper(IHtmlHelper htmlHelper)
    {
        _htmlHelper = htmlHelper;
    }

    /// <summary>
    /// An expression to be evaluated against the current model for validation messages.
    /// </summary>
    [HtmlAttributeName("asp-validation-for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// CSS classes to apply to the validation message container. Defaults to Tailwind's 'mt-1 text-sm text-red-600'.
    /// </summary>
    [HtmlAttributeName("css-class")]
    public string CssClass { get; set; } = "mt-1 text-sm text-red-600";

    /// <summary>
    /// Whether to show validation messages as a list when there are multiple errors.
    /// </summary>
    [HtmlAttributeName("show-as-list")]
    public bool ShowAsList { get; set; } = false;

    /// <summary>
    /// Custom message to display instead of the default validation message.
    /// </summary>
    [HtmlAttributeName("custom-message")]
    public string? CustomMessage { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ViewContext"/>.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        ((IViewContextAware)_htmlHelper).Contextualize(ViewContext);

        output.TagName = "div";
        output.Attributes.SetAttribute("class", CssClass);

        if (For != null)
        {
            if (!string.IsNullOrEmpty(CustomMessage))
            {
                output.Content.Append(CustomMessage);
            }
            else
            {
                var validationMessage = _htmlHelper.ValidationMessage(For.Name, "", new { });
                if (validationMessage != null)
                {
                    output.Content.AppendHtml(validationMessage);
                }
            }
        }
        else if (!string.IsNullOrEmpty(CustomMessage))
        {
            output.Content.Append(CustomMessage);
        }
    }
}