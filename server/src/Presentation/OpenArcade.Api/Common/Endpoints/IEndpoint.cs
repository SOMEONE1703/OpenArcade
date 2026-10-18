namespace OpenArcade.Api.Common.Endpoints;

/// <summary>
/// One slice's routes. Lives in <c>Features/&lt;Feature&gt;/V&lt;n&gt;/&lt;Slice&gt;/</c>; the folder decides the
/// route prefix (<c>/api/v&lt;n&gt;</c>) and the OpenAPI tag (the feature name), so slices map relative routes.
/// </summary>
public interface IEndpoint
{
    void Map(IEndpointRouteBuilder app);
}
