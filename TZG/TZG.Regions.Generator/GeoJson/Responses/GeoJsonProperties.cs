using Newtonsoft.Json;

namespace TZG.Regions.Generator.GeoJson.Responses
{
    internal sealed class GeoJsonProperties
    {
        [JsonProperty("GID_0")]
        public required string GID0 { get; init; }
        [JsonProperty("COUNTRY")]
        public required string Country { get; init; }

        [JsonProperty("GID_1")]
        public required string GID1 { get; init; }
        [JsonProperty("HASC_1")]
        public required string HASC1 { get; init; }
        [JsonProperty("NAME_1")]
        public required string NAME1 { get; init; }
        [JsonProperty("NL_NAME_1")]
        public required string NLNAME1 { get; init; }

        [JsonProperty("GID_2")]
        public required string GID2 { get; init; }
        [JsonProperty("HASC_2")]
        public required string HASC2 { get; init; }
        [JsonProperty("NAME_2")]
        public required string NAME2 { get; init; }
        [JsonProperty("NL_NAME_2")]
        public required string NLNAME2 { get; init; }

        [JsonProperty("GID_3")]
        public required string GID3 { get; init; }
        [JsonProperty("HASC_3")]
        public required string HASC3 { get; init; }
        [JsonProperty("NAME_3")]
        public required string NAME3 { get; init; }
        [JsonProperty("NL_NAME_3")]
        public required string NLNAME3 { get; init; }
    }
}
