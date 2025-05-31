using Newtonsoft.Json;

namespace TZG.Regions.Generator.Generators.Web
{
    public sealed class WebGenerator : IGenerator
    {
        private readonly string _directory;

        public WebGenerator(string directory)
        {
            _directory = directory;
        }

        public void Generate(IEnumerable<IGeoRegion> regions)
        {
            GenerateIndexes(regions);
            GenerateRegions(regions);
        }

        private void GenerateRegions(IEnumerable<IGeoRegion> regions)
        {
            foreach (var region in regions)
            {
                using var jsonWriter = new JsonTextWriter(
                    new StreamWriter(
                        Path.Combine(_directory, region.Id + ".json"),
                        new FileStreamOptions()
                        {
                            Access = FileAccess.Write,
                            Mode = FileMode.Create,
                        }
                    )
                );

                jsonWriter.WriteStartObject();

                jsonWriter.WritePropertyName("name");
                jsonWriter.WriteValue(region.Name);

                jsonWriter.WritePropertyName("local_name");
                jsonWriter.WriteValue(region.LocalName);

                jsonWriter.WritePropertyName("level");
                jsonWriter.WriteValue(region.Level.ToString());

                jsonWriter.WritePropertyName("boundaries");
                jsonWriter.WriteStartArray();

                foreach (var boundary in region.Boundaries)
                {
                    jsonWriter.WriteStartObject();

                    jsonWriter.WritePropertyName("min");
                    jsonWriter.WriteStartArray();
                    jsonWriter.WriteValue(boundary.Min.Longitude);
                    jsonWriter.WriteValue(boundary.Min.Latitude);
                    jsonWriter.WriteEndArray();

                    jsonWriter.WritePropertyName("max");
                    jsonWriter.WriteStartArray();
                    jsonWriter.WriteValue(boundary.Max.Longitude);
                    jsonWriter.WriteValue(boundary.Max.Latitude);
                    jsonWriter.WriteEndArray();

                    jsonWriter.WritePropertyName("points");
                    jsonWriter.WriteStartArray();

                    foreach (var point in boundary.Points)
                    {
                        jsonWriter.WriteStartArray();
                        jsonWriter.WriteValue(point.Longitude);
                        jsonWriter.WriteValue(point.Latitude);
                        jsonWriter.WriteEndArray();
                    }

                    jsonWriter.WriteEndArray();

                    jsonWriter.WriteEndObject();
                }

                jsonWriter.WriteEndArray();
            }
        }

        private void GenerateIndexes(IEnumerable<IGeoRegion> regions)
        {
            using var jsonWriter = new JsonTextWriter(
                new StreamWriter(
                    Path.Combine(_directory, "indexes.json"),
                    new FileStreamOptions()
                    {
                        Access = FileAccess.Write,
                        Mode = FileMode.Create,
                    }
                )
            );

            jsonWriter.WriteStartObject();

            foreach (var region in regions)
            {
                jsonWriter.WritePropertyName(region.Id);
                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue(region.Name);
                jsonWriter.WriteValue(region.LocalName);
                jsonWriter.WriteEndArray();
            }

            jsonWriter.WriteEndObject();
        }
    }
}
