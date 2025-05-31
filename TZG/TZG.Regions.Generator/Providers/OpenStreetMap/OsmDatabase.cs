namespace TZG.Regions.Generator.Providers.OpenStreetMap
{
    public sealed class OsmDatabase
    {
        public required IReadOnlyDictionary<string, OsmRegion> Regions { get; init; }
    }
}
