using System.Collections;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Clipper2Lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TZG.Regions.Generator.Providers.Gadm.Responses;

namespace TZG.Regions.Generator.Providers.Gadm
{
    internal sealed class GadmDatabase
    {
        private static readonly Regex _nameNormalizer =
            new("(Respublika|Kray|Oblast|Rayon)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _localNameNormalizer =
            new("(Республика|Край|Область|Район)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly JsonSerializer _serializer = new();

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

            private IEnumerable<GadmRegion> SubRefions => Database.Regions.Where(x => x.ParentId == _parentId);

            public IEnumerator<IGeoRegion> GetEnumerator()
            {
                return SubRefions.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public required IReadOnlyCollection<GadmRegion> Regions { get; init; }

        public static async Task<GadmDatabase> Load(string countryId, CancellationToken cancellationToken)
        {
            var apiClient = new GadmApiClient();

            var directory = Path.Combine("gadm", countryId);

            Directory.CreateDirectory(directory);

            var references = new List<IReference>();

            var regionsFile = Path.Combine(directory, "regions.json");

            IReadOnlyCollection<GadmRegion> regions;

            if (!File.Exists(regionsFile))
            {
                var countriesResponse = await apiClient.GetLevel(countryId, GeoLevel.Country, cancellationToken);
                var regionsResponse = await apiClient.GetLevel(countryId, GeoLevel.Region, cancellationToken);
                var districtsResponse = await apiClient.GetLevel(countryId, GeoLevel.District, cancellationToken);
                var subDistrictsResponse = await apiClient.GetLevel(countryId, GeoLevel.SubDistrict, cancellationToken);

                regions = countriesResponse.Features.Select(x => MapFeature(x, GeoLevel.Country, references))
                    .Concat(regionsResponse.Features.Select(x => MapFeature(x, GeoLevel.Region, references)))
                    .Concat(districtsResponse.Features.Select(x => MapFeature(x, GeoLevel.District, references)))
                    .Concat(subDistrictsResponse.Features.Select(x => MapFeature(x, GeoLevel.SubDistrict, references)))
                    .ToImmutableArray();

                using var jsonWriter = new JsonTextWriter(
                    new StreamWriter(regionsFile, new FileStreamOptions()
                    {
                        Access = FileAccess.Write,
                        Mode = FileMode.Create
                    })
                );

                _serializer.Serialize(jsonWriter, regions);
            }
            else
            {
                using var jsonReader = new JsonTextReader(new StreamReader(regionsFile));

                regions = _serializer.Deserialize<IReadOnlyCollection<GadmRegion>>(jsonReader) ??
                    throw new Exception("Invalid json");

                foreach (var region in regions)
                {
                    var subRegions = new SubRegionsReference(region.Id);

                    region.SubRegions = subRegions;

                    references.Add(subRegions);
                }
            }

            var database = new GadmDatabase()
            {
                Regions = regions
            };

            foreach (var reference in references)
            {
                reference.Database = database;
            }

            return database;
        }

        private static string NormalizeName(string name)
        {
            return _nameNormalizer.Replace(name, x => $" {x.Value} ").Trim();
        }

        private static string NormalizeLocalName(string name)
        {
            return _localNameNormalizer.Replace(name, x => $" {x.Value} ").Trim();
        }

        private static GadmRegion MapFeature(Feature feature, GeoLevel level, List<IReference> references)
        {
            var id = level switch
            {
                GeoLevel.Country => feature.Properties.GID0,
                GeoLevel.Region => feature.Properties.GID1,
                GeoLevel.District => feature.Properties.GID2,
                GeoLevel.SubDistrict => feature.Properties.GID3,
                _ => throw new NotSupportedException()
            };
            var subRegions = new SubRegionsReference(id);

            references.Add(subRegions);

            return new GadmRegion()
            {
                Id = id,
                Name = NormalizeName(level switch
                {
                    GeoLevel.Country => feature.Properties.Country,
                    GeoLevel.Region => feature.Properties.NAME1,
                    GeoLevel.District => feature.Properties.NAME2,
                    GeoLevel.SubDistrict => feature.Properties.NAME3,
                    _ => throw new NotSupportedException()
                }),
                LocalName = NormalizeLocalName(level switch
                {
                    GeoLevel.Country => feature.Properties.Country,
                    GeoLevel.Region => feature.Properties.NLNAME1,
                    GeoLevel.District => feature.Properties.NLNAME2,
                    GeoLevel.SubDistrict => feature.Properties.NLNAME3,
                    _ => throw new NotSupportedException()
                }),
                ParentId = level switch
                {
                    GeoLevel.Country => null,
                    GeoLevel.Region => feature.Properties.GID0,
                    GeoLevel.District => feature.Properties.GID1,
                    GeoLevel.SubDistrict => feature.Properties.GID2,
                    _ => throw new NotSupportedException()
                },
                Level = level,
                Boundaries = MapGeometry(feature.Geometry).ToImmutableArray(),
                SubRegions = subRegions
            };
        }

        private static IEnumerable<GeoBoundary> MapGeometry(Geometry geometry)
        {
            if (geometry.Type == "MultiPolygon")
            {
                var polygons = new PathsD();

                foreach (var boundary in geometry.Coordinates)
                {
                    foreach (var boundary2 in boundary)
                    {
                        polygons.Add(new(
                            boundary2
                                .Select(x => new PointD(x[0]!.Value<double>(), x[1]!.Value<double>()))
                                .ToImmutableArray()
                        ));
                    }
                }

                var unionPolygons = Clipper.Union(polygons, FillRule.NonZero);

                return unionPolygons.Select(
                    x => new GeoBoundary(
                        x.Select(x => new GeoPoint(x.x, x.y)).ToImmutableArray()
                    )
                );
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
