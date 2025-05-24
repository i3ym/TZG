using Newtonsoft.Json;

namespace TZG.Regions.Generator.GeoJson.Responses
{
    internal sealed class GeoJsonResponse
    {
        [JsonProperty("features")]
        public required IReadOnlyCollection<GeoJsonFeature> Features { get; init; }
    }
}
