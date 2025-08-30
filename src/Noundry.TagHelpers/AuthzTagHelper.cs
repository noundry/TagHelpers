using Microsoft.AspNetCore.Authorization;

namespace Noundry.TagHelpers;

/// <summary>
/// Suppresses rendering of an element unless specific authorization policies are met.
/// </summary>
[HtmlTargetElement("*", Attributes = AspAuthzAttributeName)]
[HtmlTargetElement("*", Attributes = AspAuthzPolicyAttributeName)]
[HtmlTargetElement("*", Attributes = AspAuthzPolicyAnyAttributeName)]
public class AuthzTagHelper : TagHelper
{
    internal static object SuppressedKey = new();
    internal static object SuppressedValue = new();

    private const string AspAuthzAttributeName = "asp-authz";
    private const string AspAuthzPolicyAttributeName = "asp-authz-policy";
    private const string AspAuthzPolicyAnyAttributeName = "asp-authz-policy-any";

    private readonly IAuthorizationService _authz;

    /// <summary>
    /// Creates a new instance of the <see cref="AuthzTagHelper" /> class.
    /// </summary>
    /// <param name="authz">The <see cref="IAuthorizationService"/>.</param>
    public AuthzTagHelper(IAuthorizationService authz)
    {
        _authz = authz;
    }

    /// <inheritdoc />
    // Run before other Tag Helpers (default Order is 0) so they can cooperatively decide not to run.
    // Note this value is coordinated with the value of IfTagHelper.Order to ensure the IfTagHelper logic runs first.
    // (Lower values run earlier).
    public override int Order => -10;

    /// <summary>
    /// A boolean indicating whether the current element requires authentication in order to be rendered.
    /// </summary>
    [HtmlAttributeName(AspAuthzAttributeName)]
    public bool RequiresAuthentication { get; set; }

    /// <summary>
    /// An authorization policy name or comma-separated list of policy names that must be satisfied in order for the current element to be rendered.
    /// When multiple policies are specified, ALL policies must be satisfied (AND logic).
    /// </summary>
    [HtmlAttributeName(AspAuthzPolicyAttributeName)]
    public string? RequiredPolicy { get; set; }

    /// <summary>
    /// An authorization policy name or comma-separated list of policy names where ANY one policy must be satisfied in order for the current element to be rendered.
    /// When multiple policies are specified, only ONE policy needs to be satisfied (OR logic).
    /// </summary>
    [HtmlAttributeName(AspAuthzPolicyAnyAttributeName)]
    public string? RequiredPolicyAny { get; set; }

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

        var requiresAuth = RequiresAuthentication || !string.IsNullOrEmpty(RequiredPolicy) || !string.IsNullOrEmpty(RequiredPolicyAny);
        var showOutput = false;

        var user = ViewContext.HttpContext.User;
        if (context.AllAttributes[AspAuthzAttributeName] != null && !requiresAuth && !user.Identity.IsAuthenticated)
        {
            // authz="false" & user isn't authenticated
            showOutput = true;
        }
        else if (!string.IsNullOrEmpty(RequiredPolicy))
        {
            // auth-policy="foo" or auth-policy="foo,bar" & user is authorized for policy/policies (ALL must be satisfied - AND logic)
            var policies = RequiredPolicy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var allAuthorized = true;

            foreach (var policy in policies)
            {
                var cacheKey = AspAuthzPolicyAttributeName + "." + policy;
                bool authorized;
                var cachedResult = ViewContext.ViewData[cacheKey];
                if (cachedResult != null)
                {
                    authorized = (bool)cachedResult;
                }
                else
                {
                    var authResult = await _authz.AuthorizeAsync(user, ViewContext, policy);
                    authorized = authResult.Succeeded;
                    ViewContext.ViewData[cacheKey] = authorized;
                }

                if (!authorized)
                {
                    allAuthorized = false;
                    break; // No need to check remaining policies if one fails
                }
            }

            showOutput = allAuthorized;
        }
        else if (!string.IsNullOrEmpty(RequiredPolicyAny))
        {
            // auth-policy-any="foo" or auth-policy-any="foo,bar" & user is authorized for ANY policy (OR logic)
            var policies = RequiredPolicyAny.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var anyAuthorized = false;

            foreach (var policy in policies)
            {
                var cacheKey = AspAuthzPolicyAnyAttributeName + "." + policy;
                bool authorized;
                var cachedResult = ViewContext.ViewData[cacheKey];
                if (cachedResult != null)
                {
                    authorized = (bool)cachedResult;
                }
                else
                {
                    var authResult = await _authz.AuthorizeAsync(user, ViewContext, policy);
                    authorized = authResult.Succeeded;
                    ViewContext.ViewData[cacheKey] = authorized;
                }

                if (authorized)
                {
                    anyAuthorized = true;
                    break; // Found one authorized policy, no need to check remaining ones
                }
            }

            showOutput = anyAuthorized;
        }
        else if (requiresAuth && user.Identity.IsAuthenticated)
        {
            // auth="true" & user is authenticated
            showOutput = true;
        }

        if (!showOutput)
        {
            output.SuppressOutput();
            context.Items[SuppressedKey] = SuppressedValue;
        }
    }
}

/// <summary>
/// Extension methods for <see cref="TagHelperContext"/>.
/// </summary>
public static class AuthzTagHelperContextExtensions
{
    /// <summary>
    /// Determines if the <see cref="AuthzTagHelper"/> (<c>asp-authz</c>) has suppressed rendering for the element associated with
    /// this <see cref="TagHelperContext"/>.
    /// </summary>
    /// <param name="context">The <see cref="TagHelperContext"/>.</param>
    /// <returns><c>true</c> if <c>asp-authz</c> suppressed rendering of this Tag Helper.</returns>
    public static bool SuppressedByAspAuthz(this TagHelperContext context) =>
        context.Items.TryGetValue(AuthzTagHelper.SuppressedKey, out var value) && value == AuthzTagHelper.SuppressedValue;
}
