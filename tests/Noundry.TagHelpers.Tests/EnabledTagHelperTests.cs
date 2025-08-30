using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noundry.TagHelpers;
using NUnit.Framework;

namespace TagHelperPack.Tests
{
    [TestFixture]
    public class EnabledTagHelperTests
    {
        [Test]
        public void IsIntanceOf_TagHelper()
        {
            var helper = new EnabledTagHelper();

            Assert.That(helper, Is.InstanceOf<TagHelper>());
        }

        [Test]
        public void IsEnabled_True_ByDefault()
        {
            var helper = new EnabledTagHelper();

            Assert.That(helper.IsEnabled, Is.True);
        }

        [TestCase(false, true)]
        [TestCase(true, false)]
        public void Process(bool isTagEnabled, bool containsDisabledAttribute)
        {
            var list = new TagHelperAttributeList();

            var context = new TagHelperContext("button", list, new Dictionary<object, object>(), "unqiueId");

            var output = new TagHelperOutput("button", list, (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
            output.TagName = "button";
            output.Content.SetHtmlContent("hello");

            var helper = new EnabledTagHelper();
            helper.IsEnabled = isTagEnabled;

            helper.Process(context, output);

            Assert.That(output.TagName, Is.EqualTo("button"));
            Assert.That(output.Attributes.ContainsName("disabled"), Is.EqualTo(containsDisabledAttribute));
        }

    }
}