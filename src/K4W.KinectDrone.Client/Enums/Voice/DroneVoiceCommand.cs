using System.ComponentModel;

namespace K4W.KinectDrone.Client.Enums.Voice
{
    public enum DroneVoiceCommand
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Takeoff")]
        TakeOff = 1,
        [Description("Land")]
        Land = 2,
        [Description("Emergency Landing")]
        EmergencyLanding = 4,
        [Description("Change video channel")]
        ChangeVideoChannel = 8,
        [Description("Shoot left")]
        ShootLeft = 16,
        [Description("Shoot right")]
        ShootRight = 32,
        [Description("Go crazy")]
        GoCrazy = 64,
        [Description("Front flip")]
        FrontFlip = 128,
        [Description("Back flip")]
        BackFlip = 256,
        [Description("Side flip")]
        SideFlip = 512
    }
}
