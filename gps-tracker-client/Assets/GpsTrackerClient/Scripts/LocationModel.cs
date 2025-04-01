using UnityEngine;

namespace GpsTrackerClient.Scripts
{
    [System.Serializable]
    public struct LocationModel
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
    
        public LocationModel(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    
        public static LocationModel operator +(LocationModel location, Vector2 step)
        {
            return new LocationModel(location.Latitude + step.x, location.Longitude + step.y);
        }
    
        public static implicit operator Vector2(LocationModel location)
        {
            return new Vector2((float)location.Latitude, (float)location.Longitude);
        }

        public override string ToString()
        {
            return $"Latitude: {Latitude}, Longitude: {Longitude}";
        }
    }
}