namespace TZG.Regions.Generator
{
    internal sealed record GeoBoundary
    {
        public GeoBoundary(IReadOnlyCollection<GeoPoint> points)
        {
            if (points.Count < 3)
                throw new Exception("Boundary must include at least 3 points");

            Points = points;

            var minLongitude = double.MaxValue;
            var minLatitude = double.MaxValue;
            var maxLongitude = double.MinValue;
            var maxLatitude = double.MinValue;

            foreach (var point in Points)
            {
                if (point.Longitude < minLongitude)
                    minLongitude = point.Longitude;

                if (point.Latitude < minLatitude)
                    minLatitude = point.Latitude;

                if (point.Longitude > maxLongitude)
                    maxLongitude = point.Longitude;

                if (point.Latitude > maxLatitude)
                    maxLatitude = point.Latitude;
            }

            Min = new GeoPoint(minLongitude, minLatitude);
            Max = new GeoPoint(maxLongitude, maxLatitude);
        }

        public GeoPoint Min { get; }
        public GeoPoint Max { get; }
        public IReadOnlyCollection<GeoPoint> Points { get; }
    }
}
