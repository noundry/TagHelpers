using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Security.Claims;
using Noundry.TagHelpers;
using NUnit.Framework;

namespace Noundry.TagHelpers.Tests;

[TestFixture]
public class AuthzTagHelperTests
{
    [Test]
    public void AuthzTagHelper_HasCorrectAttributes()
    {
        // Arrange
        var authService = new MockAuthorizationService();
        var tagHelper = new AuthzTagHelper(authService);

        // Act & Assert - Verify the tag helper has the expected attribute names
        var type = typeof(AuthzTagHelper);
        var targetElementAttributes = type.GetCustomAttributes(typeof(HtmlTargetElementAttribute), false)
            .Cast<HtmlTargetElementAttribute>()
            .ToList();

        // Should have 3 target element attributes for the three different authorization attributes
        Assert.That(targetElementAttributes.Count, Is.EqualTo(3));
        
        var attributes = targetElementAttributes.Select(attr => attr.Attributes).ToList();
        Assert.That(attributes, Contains.Item("asp-authz"));
        Assert.That(attributes, Contains.Item("asp-authz-policy"));
        Assert.That(attributes, Contains.Item("asp-authz-policy-any"));
    }

    [Test]
    public void AuthzTagHelper_HasRequiredPolicyAnyProperty()
    {
        // Arrange
        var authService = new MockAuthorizationService();
        var tagHelper = new AuthzTagHelper(authService);

        // Act & Assert - Verify the RequiredPolicyAny property exists and can be set
        tagHelper.RequiredPolicyAny = "TestPolicy,AnotherPolicy";
        Assert.That(tagHelper.RequiredPolicyAny, Is.EqualTo("TestPolicy,AnotherPolicy"));
    }

    [TestCase("Policy1", "Policy1")]
    [TestCase("Policy1,Policy2", "Policy1,Policy2")]
    [TestCase("Policy1, Policy2, Policy3", "Policy1, Policy2, Policy3")]
    public void AuthzTagHelper_RequiredPolicyAny_AcceptsValidPolicyStrings(string input, string expected)
    {
        // Arrange
        var authService = new MockAuthorizationService();
        var tagHelper = new AuthzTagHelper(authService);

        // Act
        tagHelper.RequiredPolicyAny = input;

        // Assert
        Assert.That(tagHelper.RequiredPolicyAny, Is.EqualTo(expected));
    }
}

/// <summary>
/// Mock authorization service for testing
/// </summary>
public class MockAuthorizationService : IAuthorizationService
{
    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
    {
        // Mock implementation - always returns success for testing
        return Task.FromResult(AuthorizationResult.Success());
    }

    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
    {
        // Mock implementation - always returns success for testing
        return Task.FromResult(AuthorizationResult.Success());
    }
}