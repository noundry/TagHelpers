using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Noundry.TagHelpers;

/// <summary>
/// Creates a Tailwind CSS form group with label, input, and validation message for a model property.
/// </summary>
[HtmlTargetElement("form-group")]
public class FormGroupTagHelper : TagHelper
{
    private readonly IHtmlHelper _htmlHelper;

    /// <summary>
    /// Creates a new instance of the <see cref="FormGroupTagHelper" /> class.
    /// </summary>
    /// <param name="htmlHelper">The <see cref="IHtmlHelper"/>.</param>
    public FormGroupTagHelper(IHtmlHelper htmlHelper)
    {
        _htmlHelper = htmlHelper;
    }

    /// <summary>
    /// The model expression for the form group.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public ModelExpression For { get; set; } = default!;

    /// <summary>
    /// The label text. If not provided, uses the display name from the model property.
    /// </summary>
    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Help text to display below the input.
    /// </summary>
    [HtmlAttributeName("help-text")]
    public string? HelpText { get; set; }

    /// <summary>
    /// CSS classes for the form group container. Defaults to Tailwind's 'mb-4'.
    /// </summary>
    [HtmlAttributeName("container-class")]
    public string ContainerClass { get; set; } = "mb-4";

    /// <summary>
    /// CSS classes for the label. Defaults to Tailwind's 'block text-sm font-medium text-gray-700 mb-1'.
    /// </summary>
    [HtmlAttributeName("label-class")]
    public string LabelClass { get; set; } = "block text-sm font-medium text-gray-700 mb-1";

    /// <summary>
    /// CSS classes for the input. Defaults to Tailwind's form input classes.
    /// </summary>
    [HtmlAttributeName("input-class")]
    public string InputClass { get; set; } = "block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500";

    /// <summary>
    /// Whether to include validation messages. Defaults to true.
    /// </summary>
    [HtmlAttributeName("show-validation")]
    public bool ShowValidation { get; set; } = true;

    /// <summary>
    /// Whether the field is required. If null, infers from model metadata.
    /// </summary>
    [HtmlAttributeName("required")]
    public bool? Required { get; set; }

    /// <summary>
    /// Input type for the editor (text, email, password, etc.). If not specified, uses model metadata.
    /// </summary>
    [HtmlAttributeName("input-type")]
    public string? InputType { get; set; }

    /// <summary>
    /// Placeholder text for the input.
    /// </summary>
    [HtmlAttributeName("placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ViewContext"/>.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        ((IViewContextAware)_htmlHelper).Contextualize(ViewContext);

        output.TagName = "div";
        output.Attributes.SetAttribute("class", ContainerClass);

        // Create label
        var labelText = Label ?? For.Metadata.DisplayName ?? For.Name;
        var isRequired = Required ?? For.Metadata.IsRequired;
        
        var labelContent = new TagBuilder("label");
        labelContent.Attributes["for"] = For.Name;
        labelContent.AddCssClass(LabelClass);
        labelContent.InnerHtml.Append(labelText);
        
        if (isRequired)
        {
            labelContent.InnerHtml.AppendHtml(" <span class=\"text-red-500\">*</span>");
        }
        
        output.Content.AppendHtml(labelContent);

        // Create input
        var inputAttributes = new Dictionary<string, object?>
        {
            ["class"] = InputClass,
            ["id"] = For.Name,
            ["name"] = For.Name
        };

        if (!string.IsNullOrEmpty(Placeholder))
        {
            inputAttributes["placeholder"] = Placeholder;
        }

        if (!string.IsNullOrEmpty(InputType))
        {
            inputAttributes["type"] = InputType;
        }

        if (isRequired)
        {
            inputAttributes["required"] = "required";
        }

        var editorHtml = _htmlHelper.Editor(For.Name, new { htmlAttributes = inputAttributes });
        output.Content.AppendHtml(editorHtml);

        // Add help text if provided
        if (!string.IsNullOrEmpty(HelpText))
        {
            var helpElement = new TagBuilder("div");
            helpElement.AddCssClass("mt-1 text-sm text-gray-600");
            helpElement.InnerHtml.Append(HelpText);
            output.Content.AppendHtml(helpElement);
        }

        // Add validation message if enabled
        if (ShowValidation)
        {
            var validationMessage = _htmlHelper.ValidationMessage(For.Name, "", new { @class = "mt-1 text-sm text-red-600" });
            output.Content.AppendHtml(validationMessage);
        }
    }
}