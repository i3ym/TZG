using Newtonsoft.Json;

namespace TZG.Regions.Generator.GeoJson.Responses
{
    public sealed class GeoJsonResponse
    {
        [JsonProperty("features")]
        public required IReadOnlyCollection<GeoJsonFeature> Features { get; init; }
    }
}
