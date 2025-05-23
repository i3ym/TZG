using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TZG.Regions.Generator.Providers.Gadm.Responses
{
    internal sealed class Geometry
    {
        [JsonProperty("type")]
        public required string Type { get; init; }
        [JsonProperty("coordinates")]
        public required JArray Coordinates { get; init; }
    }
}
