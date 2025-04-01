using System;

namespace GpsTrackerClient.Scripts.Services
{
    [Serializable]
    public class UserUpdateResponse : SocketResponseBase
    {
        //"type" : "userUpdate",
        // "data" : {
        //   "userId" : "406AF340-CB87-58F0-8DF1-29223F212E28",
        //   "latitude" : 27.98800056042819,
        //   "longitude" : 86.9250640699538,
        //   "timestamp" : "2025-04-01T19:53:29.907Z"
        // }

        public UserUpdateData data;
        
        public override bool IsValid => type == "userUpdate" && data != null;
    }

    [Serializable]
    public class UserUpdateData
    {
        public string userId;
        public double latitude;
        public double longitude;
        public string timestamp;
        
        public DateTime DateTime
        {
            get
            {
                if (DateTimeOffset.TryParse(this.timestamp, out var dateTimeOffset))
                {
                    return dateTimeOffset.LocalDateTime;
                }
                return DateTime.MinValue;
            }
        }
    }
}