using Newtonsoft.Json;
using System;

namespace GoogleMapsServices
{
    public class Geometry
    {
        public LatLng Location { get; set; }

        [JsonProperty("location_type")]
        public string LocationType { get; set; }

        public LatLng Viewport { get; set; }

        public Bounds Bounds { get; set; }
    }

    public class Bounds
    {
        [JsonProperty("northeast")]
        public LatLng NorthEast { get; set; }

        [JsonProperty("southwest")]
        public LatLng SouthWest { get; set; }
    }

    public class LatLng
    {
        public LatLng()
        {
            
        }

        public LatLng(double lat, double lng)
        {
            Latitude = lat;
            Longitude = lng;
        }

        public bool IsZero 
        {
            get
            {
                return Latitude == 0 && Longitude == 0;
            }
        }

        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lng")]
        public double Longitude { get; set; }
               
        public override string ToString()
        {
            return $"[Location: Latitude={Latitude}, Longitude={Longitude}]";
        }

		/// <summary>
		/// Gets the distance to another Location object in arbitrary units.
		/// This is used for distance comparison and sorting
		/// </summary>
        public double GetDistanceTo(LatLng destination)
        {
            // We deliberately ignore the curvature of the earth to simplify this equation because
            // the points that we compare will always be close enough for it not to matter.
            var latitudeDelta = Latitude - destination.Latitude;
            var longitudeDelta = Longitude - destination.Longitude;

            return Math.Sqrt((latitudeDelta * latitudeDelta) + (longitudeDelta * longitudeDelta));
        }
    }
}