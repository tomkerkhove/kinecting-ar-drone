using System.ComponentModel;

namespace K4W.KinectDrone.Core.Enums
{
    public enum DroneFlyingAction
    {
        [Description("Unkown")]
        Unknown = 0,
        [Description("Emergency")]
        Emergency = 1,
        [Description("Land")]
        Land = 2,
        [Description("Takeoff")]
        TakeOff = 4
    }
}
