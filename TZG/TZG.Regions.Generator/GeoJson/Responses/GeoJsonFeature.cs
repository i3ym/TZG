using Newtonsoft.Json;

namespace TZG.Regions.Generator.GeoJson.Responses
{
    internal sealed class GeoJsonFeature
    {
        [JsonProperty("properties")]
        public required GeoJsonProperties Properties { get; init; }
        [JsonProperty("geometry")]
        public required GeoJsonGeometry Geometry { get; init; }
    }
}
