using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Noundry.TagHelpers;
using NUnit.Framework;

[TestFixture]
public class MarkdownTagHelperTests
{
    [TestFixture]
    public class TheProcessAsyncMethod
    {
        [Test]
        public async Task RendersHtmlFromMarkdown()
        {
            var tagHelper = new MarkdownTagHelper();
            var tagHelperContext = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                Guid.NewGuid().ToString("N"));
            var tagHelperOutput = new TagHelperOutput(
                "markdown",
                new TagHelperAttributeList(),
                (_, _) => {
                    var tagHelperContent = new DefaultTagHelperContent()
                        .SetHtmlContent(new HtmlString("*Italic*, **bold**, and `monospace`."));
                    return Task.FromResult(tagHelperContent);
                });
            await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

            Assert.That(tagHelperOutput.TagName, Is.Null);
            Assert.That(tagHelperOutput.Content.GetContent(), 
                Is.EqualTo("<p><em>Italic</em>, <strong>bold</strong>, and <code>monospace</code>.</p>\n"));
        }

        [Test]
        public async Task RendersMarkdownWithHtmlEncodedByDefault()
        {
            var tagHelper = new MarkdownTagHelper { PreserveIndentation = false };
            var tagHelperContext = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                Guid.NewGuid().ToString("N"));
            var tagHelperOutput = new TagHelperOutput(
                "markdown",
                new TagHelperAttributeList(),
                (_, _) => {
                    var tagHelperContent = new DefaultTagHelperContent()
                        .SetHtmlContent(new HtmlString(@"
    # Title one

    Before image

    <img src onerror=alert(document.cookie)>

    After image

    ## Subject two

    Test"));
                    return Task.FromResult(tagHelperContent);
                });
            await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

            Assert.That(tagHelperOutput.TagName, Is.Null);
            Assert.That(tagHelperOutput.Content.GetContent(),
                Is.EqualTo(@"<h1>Title one</h1>
<p>Before image</p>
<p>&lt;img src onerror=alert(document.cookie)&gt;</p>
<p>After image</p>
<h2>Subject two</h2>
<p>Test</p>
".NormalizeLineEndings("\n")));
        }

        [Test]
        public async Task RendersSanitizedHtmlWhenAllowHtmlTrue()
        {
            var tagHelper = new MarkdownTagHelper { AllowHtml = true, PreserveIndentation = false };
            var tagHelperContext = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                Guid.NewGuid().ToString("N"));
            var tagHelperOutput = new TagHelperOutput(
                "markdown",
                new TagHelperAttributeList(),
                (_, _) => {
                    var tagHelperContent = new DefaultTagHelperContent()
                        .SetHtmlContent(new HtmlString(@"
    # Title one

    Before image

    <img src onerror=alert(document.cookie)>

    After image

    ## Subject two

    Test"));
                    return Task.FromResult(tagHelperContent);
                });
            await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

            Assert.That(tagHelperOutput.TagName, Is.Null);
            Assert.That(tagHelperOutput.Content.GetContent(),
                Is.EqualTo(@"<h1>Title one</h1>
<p>Before image</p>
<img src="""">
<p>After image</p>
<h2>Subject two</h2>
<p>Test</p>
".NormalizeLineEndings("\n")));
        }

        [Test]
        public async Task RendersMarkdownThatStartsWithIndentedCodeBlock()
        {
            var tagHelper = new MarkdownTagHelper { PreserveIndentation = true };
            var tagHelperContext = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                Guid.NewGuid().ToString("N"));
            var tagHelperOutput = new TagHelperOutput(
                "markdown",
                new TagHelperAttributeList(),
                (_, _) => {
                    var tagHelperContent = new DefaultTagHelperContent()
                        .SetHtmlContent(new HtmlString(@"
    Some Code

Not Code
"));
                    return Task.FromResult(tagHelperContent);
                });
            await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

            Assert.That(tagHelperOutput.TagName, Is.Null);
            Assert.That(tagHelperOutput.Content.GetContent(),
                Is.EqualTo(@"<pre><code>Some Code
</code></pre>
<p>Not Code</p>
".NormalizeLineEndings("\n")));
        }
    }
}