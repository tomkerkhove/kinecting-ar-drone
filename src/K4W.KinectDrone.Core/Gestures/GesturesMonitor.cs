using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;

using K4W.KinectDrone.Core.Enums;

namespace K4W.KinectDrone.Core.Gestures
{
    public sealed class GestureMonitor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="skeleton"></param>
        /// <returns></returns>
        public static DroneFlyingGesture Analyse(Skeleton skeleton)
        {
            DroneFlyingGesture result = DroneFlyingGesture.None;

            if (skeleton == null) return result;

            Joint leftHand = skeleton.Joints[JointType.HandLeft];
            Joint rightHand = skeleton.Joints[JointType.HandRight];
            Joint leftShoulder = skeleton.Joints[JointType.ShoulderLeft];
            Joint rightShoulder = skeleton.Joints[JointType.ShoulderRight];
            Joint centerHip = skeleton.Joints[JointType.HipCenter];

            double handRight_XYAngle = CalculateXYAngle(rightShoulder.Position, rightHand.Position);
            double handLeft_XYAngle = -(CalculateXYAngle(leftShoulder.Position, leftHand.Position));
            double handRight_XZAngle = CalculateXZAngle(rightShoulder.Position, rightHand.Position);
            double handLeft_XZAngle = -(CalculateXZAngle(leftShoulder.Position, leftHand.Position));

            if (leftHand.TrackingState == JointTrackingState.Tracked
                && rightHand.TrackingState == JointTrackingState.Tracked
                && leftShoulder.TrackingState == JointTrackingState.Tracked
                && rightShoulder.TrackingState == JointTrackingState.Tracked)
            {
                // Check for DroneFlyingGesture.Up
                if (handRight_XYAngle >= 25 && handLeft_XYAngle >= 25)
                    result |= DroneFlyingGesture.Up;

                // Check for DroneFlyingGesture.Down
                if (handRight_XYAngle <= -25 && handLeft_XYAngle <= -25)
                    result |= DroneFlyingGesture.Down;

                // Check for DroneFlyingGesture.Right
                if (handRight_XYAngle <= -25 && handLeft_XYAngle >= 25)
                    result |= DroneFlyingGesture.Right;

                // Check for DroneFlyingGesture.Left
                if (handRight_XYAngle >= 25 && handLeft_XYAngle <= -25)
                    result |= DroneFlyingGesture.Left;

                // Check for DroneFlyingGesture.RotateRight
                if (handRight_XZAngle >= 25 && handLeft_XZAngle <= -25)
                    result |= DroneFlyingGesture.RotateRight;

                // Check for DroneFlyingGesture.RotateLeft
                if (handRight_XZAngle <= -25 && handLeft_XZAngle >= 25)
                    result |= DroneFlyingGesture.RotateLeft;
            }

            if (leftShoulder.TrackingState == JointTrackingState.Tracked
                && rightShoulder.TrackingState == JointTrackingState.Tracked
                && centerHip.TrackingState == JointTrackingState.Tracked)
            {
                double leftShoulderDelta = centerHip.Position.Z - leftShoulder.Position.Z;
                double rightShoulderDelta = centerHip.Position.Z - rightShoulder.Position.Z;

                if (leftShoulderDelta >= .10 && rightShoulderDelta >= .10)
                    result |= DroneFlyingGesture.Forward;

                if (leftShoulderDelta <= -.10 && rightShoulderDelta <= -.10)
                    result |= DroneFlyingGesture.Backwards;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="centerPoint"></param>
        /// <param name="rotatingPoint"></param>
        /// <returns></returns>
        private static double CalculateXZAngle(SkeletonPoint centerPoint, SkeletonPoint rotatingPoint)
        {
            float equasion = (float)((rotatingPoint.Z - centerPoint.Z) / (rotatingPoint.X - centerPoint.X));
            return (Math.Tanh(equasion) * (180.0 / Math.PI));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="centerPoint"></param>
        /// <param name="rotatingPoint"></param>
        /// <returns></returns>
        private static double CalculateXYAngle(SkeletonPoint centerPoint, SkeletonPoint rotatingPoint)
        {
            float equasion = (float)((rotatingPoint.Y - centerPoint.Y) / (rotatingPoint.X - centerPoint.X));
            return (Math.Tanh(equasion) * (180.0 / Math.PI));
        }
    }
}
