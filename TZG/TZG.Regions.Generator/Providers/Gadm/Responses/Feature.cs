using Newtonsoft.Json;

namespace TZG.Regions.Generator.Providers.Gadm.Responses
{
    internal sealed class Feature
    {
        [JsonProperty("properties")]
        public required Properties Properties { get; init; }
        [JsonProperty("geometry")]
        public required Geometry Geometry { get; init; }
    }
}
