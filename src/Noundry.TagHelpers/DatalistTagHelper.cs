using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Noundry.TagHelpers;

/// <summary>
/// <see cref="ITagHelper"/> implementation targeting <c>&lt;datalist&gt;</c> elements with <c>asp-list</c> attribute(s).
/// </summary>
[HtmlTargetElement("datalist", Attributes = ListAttributeName)]
[HtmlTargetElement("datalist", Attributes = SelectListAttributeName)]
public class DatalistTagHelper : TagHelper
{
    private const string ListAttributeName = "asp-list";
    private const string SelectListAttributeName = "asp-items";

    /// <inheritdoc />
    public override int Order => -1000;

    /// <summary>
    /// A collection of <see cref="string"/> values used to populate the <c>&lt;datalist&gt;</c> element with
    /// <c>&lt;option&gt;</c> elements.
    /// </summary>
    [HtmlAttributeName(ListAttributeName)]
    public IEnumerable<string>? List { get; set; }

    /// <summary>
    /// A collection of <see cref="SelectListItem"/> objects used to populate the <c>&lt;datalist&gt;</c> element with
    /// <c>&lt;option&gt;</c> elements with both text and value attributes.
    /// </summary>
    [HtmlAttributeName(SelectListAttributeName)]
    public IEnumerable<SelectListItem>? Items { get; set; }

    /// <inheritdoc />
    /// <remarks>Does nothing if both <see cref="List"/> and <see cref="Items"/> are <c>null</c>.</remarks>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        TagBuilder? tagBuilder = null;

        // Priority: Items first, then List
        if (Items != null && Items.Any())
        {
            tagBuilder = GenerateDatalistFromSelectList(Items);
        }
        else if (List != null && List.Any())
        {
            tagBuilder = GenerateDatalist(List);
        }

        if (tagBuilder?.HasInnerHtml == true)
        {
            output.PostContent.AppendHtml(tagBuilder.InnerHtml);
        }
    }

    /// <summary>
    /// Generates a &lt;datalist&gt; element with the <paramref name="datalistList"/> as &lt;option&gt;
    /// </summary>
    /// <param name="datalistList">
    /// A collection of <see cref="string"/> objects used to populate the &lt;datalist&gt; element with
    /// &lt;option&gt; elements.
    /// </param>
    /// <remarks>Should be added to <see cref="IHtmlGenerator"/>.</remarks>
    /// <returns>A new <see cref="TagBuilder"/> describing the &lt;datalist&gt; element.</returns>
    private TagBuilder GenerateDatalist(IEnumerable<string> datalistList)
    {
        if (datalistList is not IReadOnlyCollection<string> stringList)
        {
            stringList = datalistList.ToList();
        }

        if (stringList.Count == 0)
        {
            return null;
        }

        var tagBuilder = new TagBuilder("datalist");
        var listItemBuilder = new HtmlContentBuilder(stringList.Count);
        foreach (var item in stringList)
        {
            var optionBuilder = new TagBuilder("option") { TagRenderMode = TagRenderMode.SelfClosing };
            optionBuilder.Attributes["value"] = item;
            listItemBuilder.AppendLine(optionBuilder);
        }
        tagBuilder.InnerHtml.SetHtmlContent(listItemBuilder);

        return tagBuilder;
    }

    /// <summary>
    /// Generates a &lt;datalist&gt; element with the <paramref name="selectItems"/> as &lt;option&gt; elements
    /// with both text and value attributes.
    /// </summary>
    /// <param name="selectItems">
    /// A collection of <see cref="SelectListItem"/> objects used to populate the &lt;datalist&gt; element.
    /// </param>
    /// <returns>A new <see cref="TagBuilder"/> describing the &lt;datalist&gt; element.</returns>
    private TagBuilder? GenerateDatalistFromSelectList(IEnumerable<SelectListItem> selectItems)
    {
        if (selectItems is not IReadOnlyCollection<SelectListItem> itemsList)
        {
            itemsList = selectItems.ToList();
        }

        if (itemsList.Count == 0)
        {
            return null;
        }

        var tagBuilder = new TagBuilder("datalist");
        var listItemBuilder = new HtmlContentBuilder(itemsList.Count);
        
        foreach (var item in itemsList)
        {
            var optionBuilder = new TagBuilder("option") { TagRenderMode = TagRenderMode.SelfClosing };
            optionBuilder.Attributes["value"] = item.Value;
            
            if (!string.IsNullOrEmpty(item.Text) && item.Text != item.Value)
            {
                optionBuilder.Attributes["label"] = item.Text;
            }
            
            listItemBuilder.AppendLine(optionBuilder);
        }
        
        tagBuilder.InnerHtml.SetHtmlContent(listItemBuilder);
        return tagBuilder;
    }
}
