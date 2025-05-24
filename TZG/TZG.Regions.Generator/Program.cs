using Microsoft.Extensions.Logging;
using PinkSystem.IO.Data;
using PinkSystem.Net;
using PinkSystem.Net.Http.Handlers.Factories;
using PinkSystem.Net.Sockets;
using TZG.Regions.Generator.Generators.Web;
using TZG.Regions.Generator.Http;
using TZG.Regions.Generator.Providers.Gadm;
using TZG.Regions.Generator.Providers.OpenStreetMap;

namespace TZG.Regions.Generator
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(x => x.AddConsole());

            var socketsProvider = await LimitedSocketsProvider.CreateDefault();

            IHttpRequestHandlerFactory httpRequestHandlerFactory;

            httpRequestHandlerFactory = new SystemNetNonPooledHttpRequestHandlerFactory(
                socketsProvider,
                TimeSpan.FromSeconds(60)
            );

            httpRequestHandlerFactory = new HttpReqeustHandlerFactory(
                new EnumerableDataReader<Proxy>([
                    Proxy.Parse("69b32ea3d02148ef1eb6__cr.us:3b385f70aa9837e6@gw.dataimpulse.com:823", ProxyScheme.Http)
                ]),
                httpRequestHandlerFactory,
                loggerFactory
            );

            using var databaseLoader = new OsmDatabaseLoader(
                "osm",
                httpRequestHandlerFactory.Create(new()),
                loggerFactory.CreateLogger<OsmDatabaseLoader>()
            )
            {
                MaxThreadsAmount = 100
            };

            var database = await databaseLoader.Load("-60189", CancellationToken.None);

            var regionsDirectory = Path.Combine(
                GetProjectDirectory(),
                "TZG.Web",
                "regions"
            );

            Directory.CreateDirectory(regionsDirectory);

            var generator = new WebGenerator(regionsDirectory);

            generator.Generate(database.Regions.Values);
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
}
