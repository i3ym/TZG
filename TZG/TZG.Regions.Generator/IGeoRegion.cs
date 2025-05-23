namespace TZG.Regions.Generator
{
    internal interface IGeoRegion
    {
        string Id { get; }
        string Name { get; }
        string LocalName { get; }
        GeoLevel Level { get; }
        IReadOnlyCollection<GeoBoundary> Boundaries { get; }

        IEnumerable<IGeoRegion> GetSubRegions(CancellationToken cancellationToken);
    }
}
