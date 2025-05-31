namespace TZG.Regions.Generator.Providers.Gadm
{
    public sealed class GadmDatabase
    {
        public required IReadOnlyDictionary<string, GadmRegion> Regions { get; init; }
    }
}
