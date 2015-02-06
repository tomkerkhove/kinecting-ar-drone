using System.ComponentModel;

namespace K4W.KinectDrone.Client.Enums.Voice
{
    public enum BattlestationVoiceCommand
    {
        [Description("Unknown")]
        Unknown,
        [Description("Hello BattleStation")]
        Hello,
        [Description("Goodbye BattleStation")]
        Goodbye
    }
}