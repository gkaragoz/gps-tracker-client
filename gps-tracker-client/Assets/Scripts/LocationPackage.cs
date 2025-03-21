[System.Serializable]
public struct LocationPackage
{
    public string userId;
    public double latitude;
    public double longitude;
    
    public LocationPackage(string userId, double latitude, double longitude)
    {
        this.userId = userId;
        this.latitude = latitude;
        this.longitude = longitude;
    }
}