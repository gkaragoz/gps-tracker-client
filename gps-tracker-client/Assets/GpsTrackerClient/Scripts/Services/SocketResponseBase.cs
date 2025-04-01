namespace GpsTrackerClient.Scripts.Services
{
    public abstract class SocketResponseBase
    {
        public string type;

        public abstract bool IsValid { get; }
    }
}