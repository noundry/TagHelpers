using Microsoft.AspNetCore.Razor.TagHelpers;
using Noundry.TagHelpers;
using NUnit.Framework;

namespace Noundry.TagHelpers.Tests;

[TestFixture]
public class LazyLoadingTagHelperTests
{
    [Test]
    public void Process_WithLazyTrue_AddsLoadingAttribute()
    {
        // Arrange
        var context = new TagHelperContext(
            "img",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );
        
        var output = new TagHelperOutput(
            "img",
            new TagHelperAttributeList(),
            (result, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetContent(string.Empty);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            }
        );
        
        var tagHelper = new LazyLoadingTagHelper
        {
            LazyLoad = true
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.That(output.Attributes.ContainsName("loading"), Is.True);
        Assert.That(output.Attributes["loading"].Value, Is.EqualTo("lazy"));
        Assert.That(output.Attributes.ContainsName("decoding"), Is.True);
        Assert.That(output.Attributes["decoding"].Value, Is.EqualTo("async"));
        Assert.That(output.Attributes.ContainsName("lazy"), Is.False);
    }
    
    [Test]
    public void Process_WithPlaceholder_SetsDataSrcAndPlaceholder()
    {
        // Arrange
        var context = new TagHelperContext(
            "img",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );
        
        var output = new TagHelperOutput(
            "img",
            new TagHelperAttributeList
            {
                { "src", "original.jpg" }
            },
            (result, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetContent(string.Empty);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            }
        );
        
        var tagHelper = new LazyLoadingTagHelper
        {
            LazyLoad = true,
            PlaceholderSrc = "placeholder.jpg"
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.That(output.Attributes.ContainsName("data-src"), Is.True);
        Assert.That(output.Attributes["data-src"].Value, Is.EqualTo("original.jpg"));
        Assert.That(output.Attributes["src"].Value, Is.EqualTo("placeholder.jpg"));
    }
}