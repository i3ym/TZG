namespace TZG.Regions.Generator.Generators
{
    internal interface IGenerator
    {
        void Generate(IEnumerable<IGeoRegion> regions);
    }
}
