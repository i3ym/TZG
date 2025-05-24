using System.Collections.Immutable;
using Clipper2Lib;
using Newtonsoft.Json.Linq;
using TZG.Regions.Generator.GeoJson.Responses;

namespace TZG.Regions.Generator.GeoJson
{
    internal static class GeoJsonDecoder
    {
        public static IEnumerable<GeoBoundary> DecodeGeometry(GeoJsonGeometry geometry)
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
            else if (geometry.Type == "Polygon")
            {
                var polygons = new PathsD();

                foreach (var boundary in geometry.Coordinates)
                {
                    polygons.Add(new(
                        boundary
                            .Select(x => new PointD(x[0]!.Value<double>(), x[1]!.Value<double>()))
                            .ToImmutableArray()
                    ));
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
