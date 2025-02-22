[System.Serializable]
public struct LocationPackage
{
    public string userId;
    public double latitude;
    public double longitude;
    public string searchingAreaName;
    
    public LocationPackage(string userId, double latitude, double longitude, string searchingAreaName)
    {
        this.userId = userId;
        this.latitude = latitude;
        this.longitude = longitude;
        this.searchingAreaName = searchingAreaName;
    }
}