namespace TZG.Regions.Generator.Providers.OpenStreetMap
{
    internal sealed class OsmDatabase
    {
        public required IReadOnlyDictionary<string, OsmRegion> Regions { get; init; }
    }
}
