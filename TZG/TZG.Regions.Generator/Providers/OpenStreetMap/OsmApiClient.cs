using Newtonsoft.Json;
using PinkSystem;
using PinkSystem.Net.Http.Handlers;
using TZG.Regions.Generator.GeoJson.Responses;
using TZG.Regions.Generator.Providers.OpenStreetMap.Responses;

namespace TZG.Regions.Generator.Providers.OpenStreetMap
{
    internal sealed class OsmApiClient : IDisposable
    {
        private static readonly JsonSerializer _serializer = new();
        private readonly IHttpRequestHandler _httpRequestHandler;

        public OsmApiClient(IHttpRequestHandler httpRequestHandler)
        {
            _httpRequestHandler = httpRequestHandler;
        }

        public async Task<GeoJsonGeometry> GetGeometry(string boundaryId, CancellationToken cancellationToken)
        {
            var httpResponse = await _httpRequestHandler.SendAsync(
                new PinkSystem.Net.Http.HttpRequest(
                    "GET",
                    new Uri($"https://osm-boundaries.com/api/v1/databases/osm20250407/boundaries/{boundaryId}/geometry?geometryField=way")
                ),
                cancellationToken
            );

            httpResponse.EnsureSuccessStatusCode();

            using var jsonStream = httpResponse.Content.ReadAsJsonStream();

            return _serializer.Deserialize<GeoJsonGeometry>(jsonStream) ??
                throw new Exception("Response was empty");
        }

        public async Task<IReadOnlyCollection<TreeItem>> GetTree(string rootBoundaryId, int maxDepth, CancellationToken cancellationToken)
        {
            var httpResponse = await _httpRequestHandler.SendAsync(
                new PinkSystem.Net.Http.HttpRequest(
                    "GET",
                    new Uri($"https://osm-boundaries.com/api/v1/databases/osm20250407/tree?rootBoundaryId={rootBoundaryId}&maxDepth={maxDepth}&relationsView=all")
                ),
                cancellationToken
            );

            httpResponse.EnsureSuccessStatusCode();

            using var jsonStream = httpResponse.Content.ReadAsJsonStream();

            return _serializer.Deserialize<IReadOnlyCollection<TreeItem>>(jsonStream) ??
                throw new Exception("Response was empty");
        }

        public void Dispose()
        {
            _httpRequestHandler.Dispose();
        }
    }
}
