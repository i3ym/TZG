using Newtonsoft.Json;

namespace TZG.Regions.Generator.Providers.OpenStreetMap.Responses
{
    public sealed class TreeItem
    {
        [JsonProperty("parent_boundary_id")]
        public required string ParentBoundaryId { get; init; }
        [JsonProperty("boundary_id")]
        public required string BoundaryId { get; init; }
        [JsonProperty("name")]
        public required string Name { get; init; }
        [JsonProperty("name_en")]
        public required string? NameEn { get; init; }
        [JsonProperty("boundary")]
        public required string Boundary { get; init; }
        [JsonProperty("admin_level")]
        public required int AdminLevel { get; init; }
        [JsonProperty("children")]
        public required IReadOnlyCollection<TreeItem> Children { get; init; }
    }
}
