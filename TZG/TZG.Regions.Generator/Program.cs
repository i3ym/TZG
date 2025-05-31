using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PinkSystem;
using PinkSystem.Configuration;
using PinkSystem.Net;
using PinkSystem.Net.Http;
using PinkSystem.Net.Http.Handlers;
using PinkSystem.Net.Sockets;
using TZG.Regions.Generator.Generators.Web;
using TZG.Regions.Generator.Providers.Gadm;
using TZG.Regions.Generator.Providers.OpenStreetMap;

namespace TZG.Regions.Generator
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(x => x.AddConsole());

            var logger = loggerFactory.CreateLogger(typeof(Program).FullName!);

            var configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .AddJsonFile("config.json", optional: true)
                .AddJsonFile("config.debug.json", optional: true)
                .Build();

            var socketsProvider = await LimitedSocketsProvider.CreateDefault();

            var httpRequestHandlerFactory =
                new SystemNetHttpRequestHandlerFactory(socketsProvider)
                    .WithRepeating(5, TimeSpan.FromSeconds(10), loggerFactory);

            var projectDirectory = GetProjectDirectory();

            logger.LogInformation($"Project directory {projectDirectory}");

            logger.LogInformation("Loading provider...");

            var databaseDirectory = Path.Combine(
                projectDirectory,
                "Databases"
            );

            IEnumerable<IGeoRegion> regions;

            var providerType = configuration.GetValue<string>("Provider:Type");

            switch (providerType)
            {
                case "osm":
                    var osmDatabaseDirectory = Path.Combine(
                        databaseDirectory,
                        "osm"
                    );

                    Directory.CreateDirectory(osmDatabaseDirectory);

                    var osmHttpClient = new OsmHttpClient(
                        httpRequestHandlerFactory,
                        configuration.GetValueRequired<IReadOnlyCollection<string>>("Provider:Proxies")
                            .AsDataReader()
                            .ConvertToProxy()
                    );
                    var osmApiClient = new OsmApiClient(osmHttpClient);
                    var osmDatabaseLoader = new OsmDatabaseLoader(
                        osmDatabaseDirectory,
                        osmApiClient,
                        loggerFactory.CreateLogger<OsmDatabaseLoader>()
                    )
                    {
                        MaxThreadsAmount = configuration.GetValue<int>("Provider:MaxThreadsAmount", 100)
                    };
                    var osmDatabase = await osmDatabaseLoader.Load(
                        configuration.GetValueRequired<string>("Provider:BoundaryId"),
                        CancellationToken.None
                    );

                    regions = osmDatabase.Regions.Values;
                    break;
                case "gadm":
                    var gadmDatabaseDirectory = Path.Combine(
                        databaseDirectory,
                        "gadm"
                    );

                    Directory.CreateDirectory(gadmDatabaseDirectory);

                    var gadmApiClient = new GadmApiClient(httpRequestHandlerFactory.Create());
                    var gadmDatabaseLoader = new GadmDatabaseLoader(
                        gadmDatabaseDirectory,
                        gadmApiClient,
                        loggerFactory.CreateLogger<GadmDatabaseLoader>()
                    );
                    var gadmDatabase = await gadmDatabaseLoader.Load(
                        configuration.GetValueRequired<string>("Provider:CountryId"),
                        CancellationToken.None
                    );

                    regions = gadmDatabase.Regions.Values;
                    break;
                default:
                    throw new NotSupportedException();
            }

            logger.LogInformation("Generating...");

            var regionsDirectory = Path.Combine(
                projectDirectory,
                "TZG.Web",
                "regions"
            );

            if (Directory.Exists(regionsDirectory))
                Directory.Delete(regionsDirectory, recursive: true);

            Directory.CreateDirectory(regionsDirectory);

            var generator = new WebGenerator(regionsDirectory);

            generator.Generate(regions);
        }

        private static string GetProjectDirectory()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (directory != null &&
                !directory.EnumerateDirectories().Any(x => x.Name == ".git"))
            {
                directory = directory.Parent;
            }

            if (directory == null)
                throw new Exception("Cannot find project directory");

            return directory.FullName;
        }
    }
}
