using Newtonsoft.Json;

namespace TZG.Regions.Generator.Providers.Gadm.Responses
{
    internal sealed class Response
    {
        [JsonProperty("features")]
        public required IReadOnlyCollection<Feature> Features { get; init; }
    }
}
