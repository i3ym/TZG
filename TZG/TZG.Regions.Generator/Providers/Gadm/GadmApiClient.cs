using System.IO.Compression;
using Newtonsoft.Json;
using PinkSystem.Net.Http.Handlers;
using TZG.Regions.Generator.GeoJson.Responses;

namespace TZG.Regions.Generator.Providers.Gadm
{
    public sealed class GadmApiClient : IDisposable
    {
        private static readonly JsonSerializer _serializer = new();
        private readonly IHttpRequestHandler _httpRequestHandler;

        public GadmApiClient(IHttpRequestHandler httpRequestHandler)
        {
            _httpRequestHandler = httpRequestHandler;
        }

        public async Task<GeoJsonResponse> GetLevel(string country, int level, CancellationToken cancellationToken)
        {
            var httpResponse = await _httpRequestHandler.SendAsync(
                new PinkSystem.Net.Http.HttpRequest(
                    "GET",
                    new Uri($"https://geodata.ucdavis.edu/gadm/gadm4.1/json/gadm41_{country}_{level}.json.zip")
                ),
                cancellationToken
            );

            using var archive = new ZipArchive(httpResponse.Content.CreateStream());

            using var jsonStream = new JsonTextReader(new StreamReader(archive.Entries[0].Open()));

            return _serializer.Deserialize<GeoJsonResponse>(jsonStream) ??
                throw new Exception("Response was empty");
        }

        public void Dispose()
        {
            _httpRequestHandler.Dispose();
        }
    }
}
