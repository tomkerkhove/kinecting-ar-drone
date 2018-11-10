using System.ComponentModel;

namespace K4W.KinectDrone.Core.Enums
{
    public enum DroneConnection
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Connected")]
        Connected = 1,
        [Description("Not Connected")]
        NotConnected = 2
    }
}
