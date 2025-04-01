using UnityEngine;

namespace GpsTrackerClient.Scripts
{
    public class Agent
    {
        public string UserId { get; private set; }
        public LocationModel LastKnownLocation { get; private set; }
    
        public Agent(string userId, LocationModel currentLocation)
        {
            UserId = userId;
            LastKnownLocation = new LocationModel(currentLocation.Latitude, currentLocation.Longitude);
        }
    
        public string GetLocationModelJson()
        {
            var locationModel = new LocationPackage(UserId, LastKnownLocation.Latitude, LastKnownLocation.Longitude);
            return JsonUtility.ToJson(locationModel, true);
        }
    
        public void UpdateLocation(LocationModel newLocation)
        {
            LastKnownLocation = newLocation;
        }
    }
}
