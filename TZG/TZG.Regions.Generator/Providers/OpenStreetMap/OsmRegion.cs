using Newtonsoft.Json;

namespace TZG.Regions.Generator.Providers.OpenStreetMap
{
    internal sealed class OsmRegion : IGeoRegion
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string LocalName { get; init; }
        public required int Level { get; init; }
        public required string? ParentId { get; init; }
        public required IReadOnlyCollection<GeoBoundary> Boundaries { get; init; }
        [JsonIgnore]
        public required IReadOnlyCollection<IGeoRegion> SubRegions { get; set; }

        public IEnumerable<IGeoRegion> GetSubRegions(CancellationToken cancellationToken)
        {
            return SubRegions;
        }
    }
}
