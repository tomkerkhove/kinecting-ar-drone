using System.Runtime.Remoting.Messaging;
using System.Threading;
using AR.Drone.Client;
using AR.Drone.Client.Command;
using AR.Drone.Client.Configuration;
using AR.Drone.Data;
using AR.Drone.Data.Navigation;
using AR.Drone.Infrastructure;
using AR.Drone.Video;
using K4W.KinectDrone.Core.Enums;
using System;
using System.Timers;
using PixelFormat = AR.Drone.Video.PixelFormat;
using Timer = System.Timers.Timer;

namespace K4W.KinectDrone.Core
{
    public sealed class Drone
    {
        /// <summary>
        /// Drone instance
        /// </summary>
        private DroneClient _droneClient;

        /// <summary>
        /// 
        /// </summary>
        private readonly VideoPacketDecoderWorker _decoderWorker;

        /// <summary>
        /// 
        /// </summary>
        private NavigationPacket _navigationPacket;

        /// <summary>
        /// 
        /// </summary>
        private Timer _videoTimer = new Timer(20);

        /// <summary>
        /// 
        /// </summary>
        private VideoFrame _frame;

        /// <summary>
        /// 
        /// </summary>
        private uint _frameNumber;

        /// <summary>
        /// Indication wheter the drone is airborned
        /// </summary>
        private bool _isFlying = false;
        public bool IsFlying
        {
            get { return _isFlying; }
        }

        /// <summary>
        /// 
        /// </summary>
        private DroneConnection _connection = DroneConnection.Unknown;
        public DroneConnection Connection
        {
            get { return _connection; }
        }

        /// <summary>
        /// Default CTOR
        /// </summary>
        public Drone()
        {
            // Video encoding
            _decoderWorker = new VideoPacketDecoderWorker(PixelFormat.BGR24, true, OnVideoPacketDecoded);
            _decoderWorker.Start();
            _videoTimer.Elapsed += OnVideoTimerElapsed;
            _videoTimer.Start();

            // Assign location of video DLLs
            string ffmpegPath = string.Format(@"../../../FFmpeg.AutoGen/FFmpeg/bin/windows/{0}", Environment.Is64BitProcess ? "x64" : "x86");
            ffmpegPath = System.IO.Path.GetFullPath(ffmpegPath);
            InteropHelper.RegisterLibrariesSearchPath(ffmpegPath);

            // Drone
            _droneClient = new DroneClient();
            _droneClient.NavigationPacketAcquired += OnNavigationPacketAcquired;
            _droneClient.NavigationDataAcquired += OnNavigationDataAcquired;
            _droneClient.VideoPacketAcquired += OnVideoPacketAcquired;
            _droneClient.Start();

            // Set status to not connected
            _connection = DroneConnection.NotConnected;
        }

        /// <summary>
        /// Connect to the drone
        /// </summary>
        public void Connect()
        {
            // Start drone client
            _droneClient.Start();

            // Update connection status
            CheckConnection();
        }

        /// <summary>
        /// Check the drone connection
        /// </summary>
        private void CheckConnection()
        {
            _connection = _droneClient.IsConnected ? DroneConnection.Connected : DroneConnection.NotConnected;
        }

        /// <summary>
        /// Change the flying
        /// </summary>
        public void ChangeFlyingState(DroneFlyingAction flyAction)
        {
            // Prevent same state as current
            if ((_isFlying == true && flyAction == DroneFlyingAction.TakeOff) || (_isFlying == false && flyAction != DroneFlyingAction.TakeOff))
                return;

            // Check drone status
            if (_connection != DroneConnection.Connected)
                return;

            // Show LED animation
            PerformLEDAnimation(LedAnimationType.BlinkRed, 2f, 8);

            // Execute state
            switch (flyAction)
            {
                case DroneFlyingAction.Emergency:
                    _droneClient.Emergency();
                    _isFlying = false;
                    break;

                case DroneFlyingAction.Land:
                    _droneClient.Land();
                    _isFlying = false;
                    break;

                case DroneFlyingAction.TakeOff:
                    _droneClient.Takeoff();
                    _isFlying = true;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackedGestures"></param>
        public void Control(DroneFlyingGesture trackedGestures)
        {
            if (trackedGestures == DroneFlyingGesture.Up)
                _droneClient.Progress(FlightMode.Progressive, gaz: 0.25f);

            if (trackedGestures == DroneFlyingGesture.Down)
                _droneClient.Progress(FlightMode.Progressive, gaz: -0.25f);

            if (trackedGestures == DroneFlyingGesture.RotateLeft)
                _droneClient.Progress(FlightMode.Progressive, yaw: 0.25f);

            if (trackedGestures == DroneFlyingGesture.RotateRight)
                _droneClient.Progress(FlightMode.Progressive, yaw: -0.25f);

            if (trackedGestures == DroneFlyingGesture.Left)
                _droneClient.Progress(FlightMode.Progressive, -0.05f);

            if (trackedGestures == DroneFlyingGesture.Right)
                _droneClient.Progress(FlightMode.Progressive, 0.05f);

            if (trackedGestures == DroneFlyingGesture.Forward)
                _droneClient.Progress(FlightMode.Progressive, pitch: -0.05f);

            if (trackedGestures == DroneFlyingGesture.Backwards)
                _droneClient.Progress(FlightMode.Progressive, pitch: 0.05f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ledType"></param>
        /// <param name="flightType"></param>
        public void PerformAnimation(LedAnimationType ledType, float frequency, int duration, FlightAnimationType flightType)
        {
            Settings settings = new Settings();

            settings.Leds.LedAnimation = new LedAnimation(ledType, frequency, duration);
            settings.Control.FlightAnimation = new FlightAnimation(flightType);

            _droneClient.Send(settings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ledType"></param>
        public void PerformLEDAnimation(LedAnimationType ledType)
        {
            Settings settings = new Settings();
            settings.Leds.LedAnimation = new LedAnimation(ledType, 2.0f, 2);
            _droneClient.Send(settings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ledType"></param>
        /// <param name="frequency"></param>
        /// <param name="duration"></param>
        public void PerformLEDAnimation(LedAnimationType ledType, float frequency, int duration)
        {
            Settings settings = new Settings();
            settings.Leds.LedAnimation = new LedAnimation(ledType, frequency, duration);
            _droneClient.Send(settings);
        }

        /// <summary>
        /// 
        /// </summary>
        public void PerformFlightAnimation(FlightAnimationType flightType)
        {
            Settings settings = new Settings();

            settings.Control.FlightAnimation = new FlightAnimation(flightType);

            _droneClient.Send(settings);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ChangeVideoChannel()
        {
            Settings configuration = new Settings();
            configuration.Video.Channel = VideoChannelType.Next;
            _droneClient.Send(configuration);
        }


        #region Private Events & Event bubbling

        #region Video
        private void OnVideoPacketAcquired(VideoPacket packet)
        {
            if (_decoderWorker.IsAlive)
                _decoderWorker.EnqueuePacket(packet);
        }

        private void OnVideoPacketDecoded(VideoFrame frame)
        {
            _frame = frame;
        }

        public delegate void VideoFrameAcquiredDelegate(int height, int width, byte[] imageBytes);
        public event VideoFrameAcquiredDelegate VideoFrameAcquired;
        private void OnVideoTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_frame == null || _frameNumber == _frame.Number)
                return;
            _frameNumber = _frame.Number;

            // Bubble video information if someone is listening
            if (VideoFrameAcquired != null)
                VideoFrameAcquired(_frame.Height, _frame.Width, _frame.Data);
        }
        #endregion

        #region Navigation Data
        private void OnNavigationPacketAcquired(NavigationPacket packet)
        {
            _navigationPacket = packet;
        }

        public delegate void DroneInfoAcquiredDelegate(NavigationData data);
        public event DroneInfoAcquiredDelegate DroneInfoAcquired;
        private void OnNavigationDataAcquired(NavigationData data)
        {
            // Check drone connection
            CheckConnection();

            // Bubble info
            if (DroneInfoAcquired != null)
                DroneInfoAcquired(data);
        }
        #endregion Private Events & Event bubbling

        #endregion Private Events
    }
}
