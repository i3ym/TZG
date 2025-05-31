namespace TZG.Regions.Generator
{
    public interface IGeoRegion
    {
        string Id { get; }
        string Name { get; }
        string LocalName { get; }
        int Level { get; }
        IReadOnlyCollection<GeoBoundary> Boundaries { get; }

        IEnumerable<IGeoRegion> GetSubRegions(CancellationToken cancellationToken);
    }
}
