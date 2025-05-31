using Newtonsoft.Json;
using PinkSystem;
using TZG.Regions.Generator.GeoJson.Responses;
using TZG.Regions.Generator.Providers.OpenStreetMap.Responses;

namespace TZG.Regions.Generator.Providers.OpenStreetMap
{
    public sealed class OsmApiClient
    {
        private static readonly JsonSerializer _serializer = new();
        private readonly OsmHttpClient _httpClient;

        public OsmApiClient(OsmHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GeoJsonGeometry> GetGeometry(string boundaryId, CancellationToken cancellationToken)
        {
            var httpResponse = await _httpClient.Send(
                new()
                {
                    Request = new PinkSystem.Net.Http.HttpRequest(
                        "GET",
                        new Uri($"https://osm-boundaries.com/api/v1/databases/osm20250407/boundaries/{boundaryId}/geometry?geometryField=way")
                    ),
                    UseProxy = true
                },
                cancellationToken
            );

            httpResponse.EnsureSuccessStatusCode();

            using var jsonStream = httpResponse.Content.ReadAsJsonStream();

            return _serializer.Deserialize<GeoJsonGeometry>(jsonStream) ??
                throw new Exception("Response was empty");
        }

        public async Task<IReadOnlyCollection<TreeItem>> GetTree(string rootBoundaryId, int maxDepth, CancellationToken cancellationToken)
        {
            var httpResponse = await _httpClient.Send(
                new()
                {
                    Request = new PinkSystem.Net.Http.HttpRequest(
                        "GET",
                        new Uri($"https://osm-boundaries.com/api/v1/databases/osm20250407/tree?rootBoundaryId={rootBoundaryId}&maxDepth={maxDepth}&relationsView=all")
                    )
                },
                cancellationToken
            );

            httpResponse.EnsureSuccessStatusCode();

            using var jsonStream = httpResponse.Content.ReadAsJsonStream();

            return _serializer.Deserialize<IReadOnlyCollection<TreeItem>>(jsonStream) ??
                throw new Exception("Response was empty");
        }
    }
}
