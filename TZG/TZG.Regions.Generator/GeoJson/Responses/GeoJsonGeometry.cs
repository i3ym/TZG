using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TZG.Regions.Generator.GeoJson.Responses
{
    internal sealed class GeoJsonGeometry
    {
        [JsonProperty("type")]
        public required string Type { get; init; }
        [JsonProperty("coordinates")]
        public required JArray Coordinates { get; init; }
    }
}
