using UnityEngine;

public class Agent
{
    public string UserId { get; private set; }
    public string SearchingAreaName { get; private set; }
    public float SearchRadius { get; private set; }
    public LocationModel LastKnownLocation { get; private set; }
    
    public Agent(string userId, string searchingAreaName, LocationModel currentLocation, float searchRadius)
    {
        UserId = userId;
        SearchingAreaName = searchingAreaName;
        LastKnownLocation = new LocationModel(currentLocation.Latitude, currentLocation.Longitude);
        SearchRadius = searchRadius;
    }
    
    public string GetLocationModelJson()
    {
        var locationModel = new LocationPackage(UserId, LastKnownLocation.Latitude, LastKnownLocation.Longitude, SearchingAreaName);
        return JsonUtility.ToJson(locationModel);
    }

    public void SearchOneStep()
    {
        LastKnownLocation += GetRandomStep();
    }
    
    private Vector2 GetRandomStep()
    {
        return new Vector2(Random.Range(-0.0001f, 0.0001f), Random.Range(-0.0001f, 0.0001f));
    }
}
