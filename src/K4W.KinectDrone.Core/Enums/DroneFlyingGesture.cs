using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K4W.KinectDrone.Core.Enums
{
    public enum DroneFlyingGesture
    {
        None = 0,
        Up = 1,
        Down = 2,
        Forward = 4,
        Backwards = 8,
        Left = 16,
        Right = 32,
        RotateLeft = 64,
        RotateRight = 128
    }
}
