using Microsoft.AspNetCore.Razor.TagHelpers;
using Noundry.TagHelpers;
using NUnit.Framework;

namespace Noundry.TagHelpers.Tests;

[TestFixture]
public class AutoHideTagHelperTests
{
    [Test]
    public async Task AutoHideTagHelper_AddsIdWhenMissing()
    {
        // Arrange
        var context = new TagHelperContext(
            "div",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );
        
        var output = new TagHelperOutput(
            "div",
            new TagHelperAttributeList(),
            (result, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetContent("Test content");
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            }
        );
        
        var tagHelper = new AutoHideTagHelper
        {
            AutoHide = true,
            DelayInSeconds = 3
        };

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.That(output.Attributes.ContainsName("id"), Is.True);
        Assert.That(output.Attributes["id"].Value.ToString(), Does.StartWith("auto-hide-"));
        Assert.That(output.PostContent.GetContent(), Does.Contain("<script>"));
        Assert.That(output.PostContent.GetContent(), Does.Contain("setTimeout"));
    }

    [Test]
    public async Task AutoHideTagHelper_PreservesExistingId()
    {
        // Arrange
        var context = new TagHelperContext(
            "div",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );
        
        var output = new TagHelperOutput(
            "div",
            new TagHelperAttributeList
            {
                { "id", "existing-id" }
            },
            (result, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetContent("Test content");
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            }
        );
        
        var tagHelper = new AutoHideTagHelper
        {
            AutoHide = true
        };

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.That(output.Attributes["id"].Value, Is.EqualTo("existing-id"));
        Assert.That(output.PostContent.GetContent(), Does.Contain("getElementById('existing-id')"));
    }

    [Test]
    public async Task AutoHideTagHelper_SetsDefaultTagAndClass()
    {
        // Arrange
        var context = new TagHelperContext(
            "asp-auto-hide",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );
        
        var output = new TagHelperOutput(
            "asp-auto-hide",
            new TagHelperAttributeList(),
            (result, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetContent("Auto-hide content");
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            }
        );
        
        var tagHelper = new AutoHideTagHelper
        {
            AutoHide = true
        };

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.That(output.TagName, Is.EqualTo("div"));
        Assert.That(output.Attributes["class"].Value, Is.EqualTo("p-4 rounded-lg border"));
    }

    [TestCase(3, "3s")]
    [TestCase(10, "10s")]
    [TestCase(1, "1s")]
    public async Task AutoHideTagHelper_UsesCorrectDelay(int delaySeconds, string expectedLog)
    {
        // Arrange
        var context = new TagHelperContext(
            "div",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );
        
        var output = new TagHelperOutput(
            "div",
            new TagHelperAttributeList(),
            (result, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetContent("Test");
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            }
        );
        
        var tagHelper = new AutoHideTagHelper
        {
            AutoHide = true,
            DelayInSeconds = delaySeconds,
            Debug = true
        };

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var scriptContent = output.PostContent.GetContent();
        Assert.That(scriptContent, Does.Contain($"setTimeout(hideElement, {delaySeconds * 1000})"));
        Assert.That(scriptContent, Does.Contain($"Starting hide timer: {delaySeconds}s"));
    }

    [TestCase("fade", "opacity = '0'")]
    [TestCase("slide-up", "translateY(-100%)")]
    [TestCase("scale", "scale(0)")]
    public async Task AutoHideTagHelper_UsesCorrectEffect(string effect, string expectedTransform)
    {
        // Arrange
        var context = new TagHelperContext(
            "div",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );
        
        var output = new TagHelperOutput(
            "div",
            new TagHelperAttributeList(),
            (result, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetContent("Test");
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            }
        );
        
        var tagHelper = new AutoHideTagHelper
        {
            AutoHide = true,
            Effect = effect
        };

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var scriptContent = output.PostContent.GetContent();
        Assert.That(scriptContent, Does.Contain(expectedTransform));
    }

    [Test]
    public async Task AutoHideTagHelper_DoesNothingWhenDisabled()
    {
        // Arrange
        var context = new TagHelperContext(
            "div",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );
        
        var output = new TagHelperOutput(
            "div",
            new TagHelperAttributeList(),
            (result, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetContent("Test content");
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            }
        );
        
        var tagHelper = new AutoHideTagHelper
        {
            AutoHide = false
        };

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.That(output.PostContent.GetContent(), Does.Not.Contain("<script>"));
        Assert.That(output.PostContent.GetContent(), Is.Empty);
    }
}