using System.IO.Compression;
using Newtonsoft.Json;
using TZG.Regions.Generator.Providers.Gadm.Responses;

namespace TZG.Regions.Generator.Providers.Gadm
{
    internal sealed class GadmApiClient : IDisposable
    {
        private static readonly JsonSerializer _serializer = new();
        private readonly HttpClient _httpClient = new()
        {
            Timeout = Timeout.InfiniteTimeSpan,
            DefaultRequestHeaders =
            {
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:138.0) Gecko/20100101 Firefox/138.0" }
            }
        };

        public async Task<Response> GetLevel(string country, GeoLevel level, CancellationToken cancellationToken)
        {
            var levelNumber = level switch
            {
                GeoLevel.Country => 0,
                GeoLevel.Region => 1,
                GeoLevel.District => 2,
                GeoLevel.SubDistrict => 3,
                _ => throw new NotSupportedException()
            };

            using var remoteStream = new ZipArchive(
                await _httpClient.GetStreamAsync(
                    new Uri($"https://geodata.ucdavis.edu/gadm/gadm4.1/json/gadm41_{country}_{levelNumber}.json.zip"),
                    cancellationToken
                )
            );

            using var entryStream = remoteStream.Entries[0].Open();

            var jsonStream = new JsonTextReader(new StreamReader(entryStream));

            return _serializer.Deserialize<Response>(jsonStream) ??
                throw new Exception("Response was empty");
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
