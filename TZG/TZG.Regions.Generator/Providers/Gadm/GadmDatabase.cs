namespace TZG.Regions.Generator.Providers.Gadm
{
    internal sealed class GadmDatabase
    {
        public required IReadOnlyDictionary<string, GadmRegion> Regions { get; init; }
    }
}
