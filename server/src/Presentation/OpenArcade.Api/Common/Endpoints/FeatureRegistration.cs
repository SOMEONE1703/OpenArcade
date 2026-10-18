using System.Reflection;
using System.Text.RegularExpressions;

namespace OpenArcade.Api.Common.Endpoints;

public static partial class FeatureRegistration
{
    private const string FeaturesNamespace = "OpenArcade.Api.Features.";

    private static IEnumerable<Type> FeatureTypes => Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => t is { IsClass: true, IsAbstract: false } && t.Namespace?.StartsWith(FeaturesNamespace) == true);

    /// <summary>Registers every slice's handler (classes named *Handler under Features) as scoped.</summary>
    public static IServiceCollection AddFeatureHandlers(this IServiceCollection services)
    {
        foreach (var type in FeatureTypes.Where(t => t.Name.EndsWith("Handler")))
            services.AddScoped(type);
        return services;
    }

    /// <summary>Maps every <see cref="IEndpoint"/> under <c>/api/v&lt;n&gt;</c>, taking n from its V&lt;n&gt; folder.</summary>
    public static IEndpointRouteBuilder MapFeatureEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = FeatureTypes.Where(t => typeof(IEndpoint).IsAssignableFrom(t)).Select(t =>
        {
            var segments = t.Namespace![FeaturesNamespace.Length..].Split('.');
            var version = segments.Select(s => VersionSegment().Match(s)).FirstOrDefault(m => m.Success)
                ?? throw new InvalidOperationException($"{t.FullName} must live in a V<n> folder.");
            return (Type: t, Feature: segments[0], Version: int.Parse(version.Groups[1].Value));
        });

        foreach (var group in endpoints.GroupBy(e => (e.Feature, e.Version)))
        {
            var routes = app.MapGroup($"/api/v{group.Key.Version}").WithTags(group.Key.Feature);
            foreach (var e in group)
                ((IEndpoint)Activator.CreateInstance(e.Type)!).Map(routes);
        }
        return app;
    }

    [GeneratedRegex(@"^V(\d+)$")]
    private static partial Regex VersionSegment();
}
