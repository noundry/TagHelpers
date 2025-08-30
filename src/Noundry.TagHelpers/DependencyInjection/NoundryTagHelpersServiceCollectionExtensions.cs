using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Noundry.TagHelpers;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for Noundry.TagHelpers.
/// </summary>
public static class NoundryTagHelpersServiceCollectionExtensions
{
    /// <summary>
    /// Add optional services to optimize Noundry.TagHelpers.
    /// <list type="bullet">
    ///   <item>Registers <see cref="ModelHtmlHelper"/> as <see cref="IHtmlHelper"/> and <see cref="IModelHtmlHelper"/>.</item>
    /// </list>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns><paramref name="services"/></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddNoundryTagHelpers(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddTransient<IHtmlHelper, ModelHtmlHelper>();
        services.AddTransient<IModelHtmlHelper, ModelHtmlHelper>();

        return services;
    }
}
