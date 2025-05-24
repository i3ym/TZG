using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PinkSystem.Net.Http.Handlers;
using TZG.Regions.Generator.GeoJson;
using TZG.Regions.Generator.GeoJson.Responses;

namespace TZG.Regions.Generator.Providers.Gadm
{
    internal sealed class GadmDatabaseLoader : IDisposable
    {
        private static readonly Regex _nameNormalizer =
            new("(Respublika|Kray|Oblast|Rayon)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _localNameNormalizer =
            new("(Республика|Край|Область|Район)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly JsonSerializer _serializer = new();
        private readonly string _directory;
        private readonly GadmApiClient _apiClient;
        private readonly ILogger<GadmDatabaseLoader> _logger;
        private ConcurrentBag<IReference>? _references;

        private interface IReference
        {
            GadmDatabase Database { set; }
        }

        private sealed class SubRegionsReference : IReadOnlyCollection<IGeoRegion>, IReference
        {
            private readonly string _parentId;
            private GadmDatabase? _database;

            public SubRegionsReference(string parentId)
            {
                _parentId = parentId;
            }

            public GadmDatabase Database
            {
                get => _database ?? throw new Exception("Database not initialized");
                set => _database = value;
            }

            public int Count => SubRefions.Count();

            private IEnumerable<GadmRegion> SubRefions => Database.Regions.Values.Where(x => x.ParentId == _parentId);

            public IEnumerator<IGeoRegion> GetEnumerator()
            {
                return SubRefions.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public GadmDatabaseLoader(
            string directory,
            IHttpRequestHandler httpRequestHandler,
            ILogger<GadmDatabaseLoader> logger
        )
        {
            _directory = directory;
            _apiClient = new(httpRequestHandler);
            _logger = logger;
        }

        private ConcurrentBag<IReference> References => _references ?? throw new Exception("References not initialized");

        public async Task<GadmDatabase> Load(string countryId, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(_directory);

            _references = new();

            var regionsFile = Path.Combine(_directory, countryId + ".json");

            IReadOnlyDictionary<string, GadmRegion> regions;

            if (!File.Exists(regionsFile))
            {
                _logger.LogInformation("Loading countries...");

                var countriesResponse = await _apiClient.GetLevel(countryId, GadmLevel.Country, cancellationToken);

                _logger.LogInformation("Loading regions...");

                var regionsResponse = await _apiClient.GetLevel(countryId, GadmLevel.Region, cancellationToken);

                _logger.LogInformation("Loading districts...");

                var districtsResponse = await _apiClient.GetLevel(countryId, GadmLevel.District, cancellationToken);

                _logger.LogInformation("Loading sub districts...");

                var subDistrictsResponse = await _apiClient.GetLevel(countryId, GadmLevel.SubDistrict, cancellationToken);

                regions = countriesResponse.Features.Select(x => GetRegion(x, GadmLevel.Country))
                    .Concat(regionsResponse.Features.Select(x => GetRegion(x, GadmLevel.Region)))
                    .Concat(districtsResponse.Features.Select(x => GetRegion(x, GadmLevel.District)))
                    .Concat(subDistrictsResponse.Features.Select(x => GetRegion(x, GadmLevel.SubDistrict)))
                    .ToImmutableDictionary(x => x.Id);

                using var jsonWriter = new JsonTextWriter(
                    new StreamWriter(regionsFile, new FileStreamOptions()
                    {
                        Access = FileAccess.Write,
                        Mode = FileMode.Create
                    })
                );

                _serializer.Serialize(jsonWriter, regions);

                _logger.LogInformation("Writing local database...");
            }
            else
            {
                _logger.LogInformation("Reading local database...");

                using var jsonReader = new JsonTextReader(new StreamReader(regionsFile));

                regions = _serializer.Deserialize<IReadOnlyDictionary<string, GadmRegion>>(jsonReader) ??
                    throw new Exception("Invalid json");

                foreach (var region in regions)
                {
                    var subRegions = new SubRegionsReference(region.Key);

                    region.Value.SubRegions = subRegions;

                    _references.Add(subRegions);
                }
            }

            var database = new GadmDatabase()
            {
                Regions = regions
            };

            foreach (var reference in _references)
            {
                reference.Database = database;
            }

            return database;
        }

        private GadmRegion GetRegion(GeoJsonFeature feature, int level)
        {
            var id = level switch
            {
                GadmLevel.Country => feature.Properties.GID0,
                GadmLevel.Region => feature.Properties.GID1,
                GadmLevel.District => feature.Properties.GID2,
                GadmLevel.SubDistrict => feature.Properties.GID3,
                _ => throw new NotSupportedException()
            };
            var subRegions = new SubRegionsReference(id);

            References.Add(subRegions);

            return new GadmRegion()
            {
                Id = id,
                Name = NormalizeName(level switch
                {
                    GadmLevel.Country => feature.Properties.Country,
                    GadmLevel.Region => feature.Properties.NAME1,
                    GadmLevel.District => feature.Properties.NAME2,
                    GadmLevel.SubDistrict => feature.Properties.NAME3,
                    _ => throw new NotSupportedException()
                }),
                LocalName = NormalizeLocalName(level switch
                {
                    GadmLevel.Country => feature.Properties.Country,
                    GadmLevel.Region => feature.Properties.NLNAME1,
                    GadmLevel.District => feature.Properties.NLNAME2,
                    GadmLevel.SubDistrict => feature.Properties.NLNAME3,
                    _ => throw new NotSupportedException()
                }),
                ParentId = level switch
                {
                    GadmLevel.Country => null,
                    GadmLevel.Region => feature.Properties.GID0,
                    GadmLevel.District => feature.Properties.GID1,
                    GadmLevel.SubDistrict => feature.Properties.GID2,
                    _ => throw new NotSupportedException()
                },
                Level = level,
                Boundaries = GeoJsonDecoder.DecodeGeometry(feature.Geometry).ToImmutableArray(),
                SubRegions = subRegions
            };
        }

        private static string NormalizeName(string name)
        {
            return _nameNormalizer.Replace(name, x => $" {x.Value} ").Trim();
        }

        private static string NormalizeLocalName(string name)
        {
            return _localNameNormalizer.Replace(name, x => $" {x.Value} ").Trim();
        }

        public void Dispose()
        {
            _apiClient.Dispose();
        }
    }
}
