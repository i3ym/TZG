using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TZG.Regions.Generator.GeoJson;
using System.Collections;
using TZG.Regions.Generator.Providers.OpenStreetMap.Responses;
using System.Collections.Immutable;
using NickBuhro.Translit;
using System.Collections.Concurrent;
using PinkSystem;
using PinkSystem.Net.Http.Handlers;

namespace TZG.Regions.Generator.Providers.OpenStreetMap
{
    internal sealed class OsmDatabaseLoader : IDisposable
    {
        private static readonly JsonSerializer _serializer = new();
        private readonly string _directory;
        private readonly OsmApiClient _apiClient;
        private readonly ILogger<OsmDatabaseLoader> _logger;
        private ConcurrentBag<IReference>? _references;

        private interface IReference
        {
            OsmDatabase Database { set; }
        }

        private sealed class SubRegionsReference : IReadOnlyCollection<IGeoRegion>, IReference
        {
            private readonly string _parentId;
            private OsmDatabase? _database;

            public SubRegionsReference(string parentId)
            {
                _parentId = parentId;
            }

            public OsmDatabase Database
            {
                get => _database ?? throw new Exception("Database not initialized");
                set => _database = value;
            }

            public int Count => SubRefions.Count();

            private IEnumerable<IGeoRegion> SubRefions => Database.Regions.Values.Where(x => x.ParentId == _parentId);

            public IEnumerator<IGeoRegion> GetEnumerator()
            {
                return SubRefions.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public OsmDatabaseLoader(
            string directory,
            IHttpRequestHandler httpRequestHandler,
            ILogger<OsmDatabaseLoader> logger
        )
        {
            _directory = directory;
            _apiClient = new(httpRequestHandler);
            _logger = logger;
        }

        public int MaxThreadsAmount { get; set; } = 10;
        private ConcurrentBag<IReference> References => _references ?? throw new Exception("References not initialized");

        public async Task<OsmDatabase> Load(string boundaryId, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(_directory);

            _references = new();
            
            var tasksPool = new TasksPool(MaxThreadsAmount, cancellationToken);

            _logger.LogInformation("Loading tree...");

            var treeItems = await _apiClient.GetTree(boundaryId, maxDepth: 50, cancellationToken);
            var flatTreeItems = ToFlat(treeItems).ToImmutableArray();

            var regions = new ConcurrentDictionary<string, OsmRegion>(-1, treeItems.Count);

            var index = 0;

            foreach (var treeItem in flatTreeItems)
            {
                var currentIndex = ++index;

                await tasksPool.WaitAny();

                tasksPool.StartNew(async (cancellationToken) =>
                {
                    var region = await GetRegion(treeItem, cancellationToken);

                    regions.TryAdd(region.Id, region);
                });
            }

            await tasksPool.WaitAll();

            foreach (var region in regions)
            {
                var subRegions = new SubRegionsReference(region.Key);

                region.Value.SubRegions = subRegions;

                _references.Add(subRegions);
            }

            var database = new OsmDatabase()
            {
                Regions = regions
            };

            foreach (var reference in _references)
            {
                reference.Database = database;
            }

            return database;
        }

        private async Task<OsmRegion> GetRegion(TreeItem treeItem, CancellationToken cancellationToken)
        {
            var regionFile = Path.Combine(_directory, treeItem.BoundaryId);

            if (!File.Exists(regionFile))
            {
                var region = await LoadRegion(treeItem, cancellationToken);

                using var jsonWriter = new JsonTextWriter(
                    new StreamWriter(regionFile, new FileStreamOptions()
                    {
                        Access = FileAccess.Write,
                        Mode = FileMode.Create
                    })
                );

                _serializer.Serialize(jsonWriter, region);

                return region;
            }
            else
            {
                _logger.LogInformation($"Reading geometry for region {treeItem.BoundaryId}...");

                using var jsonReader = new JsonTextReader(new StreamReader(regionFile));

                var region = _serializer.Deserialize<OsmRegion>(jsonReader) ??
                    throw new Exception("Invalid json");

                return region;
            }
        }

        private async Task<OsmRegion> LoadRegion(TreeItem treeItem, CancellationToken cancellationToken)
        {
            var name = treeItem.NameEn == null ?
                Transliteration.CyrillicToLatin(treeItem.Name) :
                treeItem.NameEn;

            _logger.LogInformation($"Loading geometry for region {treeItem.BoundaryId} ({name})...");

            var subRegions = new SubRegionsReference(treeItem.BoundaryId);

            while (true)
            {
                try
                {
                    var geometry = await _apiClient.GetGeometry(treeItem.BoundaryId, cancellationToken);
                    var boundaries = GeoJsonDecoder.DecodeGeometry(geometry).ToImmutableArray();

                    var region = new OsmRegion()
                    {
                        Id = treeItem.BoundaryId,
                        Name = name,
                        LocalName = treeItem.Name,
                        ParentId = treeItem.ParentBoundaryId,
                        Level = treeItem.AdminLevel,
                        Boundaries = boundaries,
                        SubRegions = subRegions
                    };

                    References.Add(subRegions);

                    return region;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, $"Error when loading geometry for region {treeItem.BoundaryId} ({name}). Retrying...");
                }
            }
        }

        private static IEnumerable<TreeItem> ToFlat(IEnumerable<TreeItem> treeItems)
        {
            foreach (var treeItem in treeItems)
            {
                yield return treeItem;

                foreach (var childTreeItem in ToFlat(treeItem.Children))
                    yield return childTreeItem;
            }
        }

        public void Dispose()
        {
            _apiClient.Dispose();
        }
    }
}
