using TZG.Regions.Generator.Generators.Web;
using TZG.Regions.Generator.Providers.Gadm;

namespace TZG.Regions.Generator;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var gadmDatabase = await GadmDatabase.Load("RUS", CancellationToken.None);

        var regionsDirectory = Path.Combine(
            GetProjectDirectory(),
            "TZG.Web",
            "regions"
        );

        Directory.CreateDirectory(regionsDirectory);

        var generator = new WebGenerator(regionsDirectory);

        generator.Generate(gadmDatabase.Regions);
    }

    private static string GetProjectDirectory()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory != null && !directory.EnumerateDirectories().Any(x => x.Name == ".git"))
        {
            directory = directory.Parent;
        }

        if (directory == null)
            throw new Exception("Cannot find project directory");

        return directory.FullName;
    }
}
