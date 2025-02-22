[System.Serializable]
public struct LocationModel
{
    public string userId;
    public double latitude;
    public double longitude;
    
    public LocationModel(string userId, double latitude, double longitude)
    {
        this.userId = userId;
        this.latitude = latitude;
        this.longitude = longitude;
    }
}