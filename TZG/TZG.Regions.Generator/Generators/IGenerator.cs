namespace TZG.Regions.Generator.Generators
{
    public interface IGenerator
    {
        void Generate(IEnumerable<IGeoRegion> regions);
    }
}
