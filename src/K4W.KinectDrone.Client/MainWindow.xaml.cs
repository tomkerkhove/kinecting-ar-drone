using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AR.Drone.Client.Configuration;
using K4W.KinectDrone.Core.Gestures;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

using Microsoft.Kinect;

using K4W.KinectDrone.Core;
using K4W.KinectDrone.Core.Enums;
using K4W.KinectDrone.Core.Speech;

using K4W.KinectDrone.Client.Enums.Voice;
using K4W.KinectDrone.Client.Extensions;

using AR.Drone.Client;
using AR.Drone.Data.Navigation;

namespace K4W.KinectDrone.Client
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Representation of our Kinect-sensor
        /// </summary>
        private KinectSensor _currentSensor = null;

        /// <summary>
        /// Representation of our Drone-sensor
        /// </summary>
        private Drone _drone = null;

        /// <summary>
        /// WritebleBitmap that will draw the Kinect video output
        /// </summary>
        private WriteableBitmap _cameraVision = null;

        /// <summary>
        /// WritebleBitmap that will draw the Drone video output
        /// </summary>
        private WriteableBitmap _droneVideo = null;

        /// <summary>
        /// Buffer to copy the pixel data to
        /// </summary>
        private byte[] _kinectPixelData = new byte[0];

        /// <summary>
        /// Collection of skeletons
        /// </summary>
        private Skeleton[] _skeletons = new Skeleton[6];

        /// <summary>
        /// The RecognitionEngine used to build our grammar and start recognizing
        /// </summary>
        private SpeechRecognitionEngine _recognizer;

        /// <summary>
        /// The KinectAudioSource that is used.
        /// Basicly gets the Audio from the microphone array
        /// </summary>
        private KinectAudioSource _audioSource;

        /// <summary>
        /// Timestamp of the last successfully recognized command
        /// </summary>
        private DateTime _lastCommand = DateTime.Now;

        /// <summary>
        /// A constant defining the delay between 2 successful voice recognitions.
        /// It will be dropped if there already is one recognized in this interval
        /// </summary>
        private const float _delayInSeconds = 2;

        /// <summary>
        /// All speech commands and actions
        /// </summary>
        private readonly Dictionary<string, object> _battlestationCommands = new Dictionary<string, object>()
        {
            { "Hello", BattlestationVoiceCommand.Hello },
            { "Goodbye", BattlestationVoiceCommand.Goodbye }
        };

        /// <summary>
        /// All speech commands for controlling our drone
        /// </summary>
        private readonly Dictionary<string, object> _droneCommands = new Dictionary<string, object>()
        {
            { "Take off", DroneVoiceCommand.TakeOff },
            { "Land", DroneVoiceCommand.Land },
            { "May day", DroneVoiceCommand.EmergencyLanding },
            { "Change the channel", DroneVoiceCommand.ChangeVideoChannel },
            { "Fire left missle", DroneVoiceCommand.ShootLeft },
            { "Fire right missle", DroneVoiceCommand.ShootRight },
            { "Front flip", DroneVoiceCommand.FrontFlip },
            { "Back flip", DroneVoiceCommand.BackFlip },
            { "Side flip", DroneVoiceCommand.SideFlip },
            { "Go crazy", DroneVoiceCommand.GoCrazy }
        };


        /// <summary>
        /// Default CTOR
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Kinect init
            InitializeKinect();

            // Drone Init
            InitializeDrone();

            Loaded += OnLoaded;

            // General events
            Closing += OnClosing;
        }

        /// <summary>
        /// Introduction
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Narrator.Speak("Initializing battle station");
        }

        /// <summary>
        /// Stop Kinect when closing the application
        /// </summary>
        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (_currentSensor != null && _currentSensor.Status == KinectStatus.Connected)
                _currentSensor.Stop();
        }


        #region Kinect Global
        /// <summary>
        /// Initialisation of the Kinect
        /// </summary>
        private void InitializeKinect()
        {
            // Get current running sensor
            KinectSensor sensor = KinectSensor.KinectSensors.FirstOrDefault(sens => sens.Status == KinectStatus.Connected);

            // Initialize sensor
            StartSensor(sensor);

            // Sub to Kinect StatusChanged-event
            KinectSensor.KinectSensors.StatusChanged += OnKinectStatusChanged;
        }

        /// <summary>
        /// Start a new sensor
        /// </summary>
        private void StartSensor(KinectSensor sensor)
        {
            // Avoid crashes
            if (sensor == null)
                return;

            // Save instance
            _currentSensor = sensor;

            // Initialize color & skeletal tracking
            _currentSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            _currentSensor.SkeletonStream.Enable();

            // Sub to events
            _currentSensor.AllFramesReady += OnAllFramesReadyHandler;

            // Start sensor
            _currentSensor.Start();

            // Save sensor status
            KinectStatus = _currentSensor.Status;

            // Initialize speech
            InitializeSpeech();
        }

        /// <summary>
        /// Process a Kinect status change
        /// </summary>
        private void OnKinectStatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (_currentSensor == null || _currentSensor.DeviceConnectionId != e.Sensor.DeviceConnectionId)
                return;

            // Save new status
            KinectStatus = e.Sensor.Status;

            // More later
        }
        #endregion Kinect Global

        #region Kinect Speech
        /// <summary>
        /// Initialize speech
        /// </summary>
        private void InitializeSpeech()
        {
            // Check if vocabulary is specifief
            if (_droneCommands == null || _droneCommands.Count == 0)
                throw new ArgumentException("A vocabulary is required.");

            // Check sensor state
            if (_currentSensor.Status != KinectStatus.Connected)
                throw new Exception("Unable to initialize speech if sensor isn't connected.");

            // Get the RecognizerInfo of our Kinect sensor
            RecognizerInfo info = GetKinectRecognizer();

            // Let user know if there is none.
            if (info == null)
                throw new Exception("There was a problem initializing Speech Recognition.\nEnsure that you have the Microsoft Speech SDK installed.");

            // Create new speech-engine
            try
            {
                _recognizer = new SpeechRecognitionEngine(info.Id);

                if (_recognizer == null) throw new Exception();
            }
            catch (Exception ex)
            {
                throw new Exception("There was a problem initializing Speech Recognition.\nEnsure that you have the Microsoft Speech SDK installed.");
            }

            #region Global speech CMD grammar

            // Add our speech hello/goodbye to start listening
            Choices battlestationChoices = new Choices();
            foreach (string key in _battlestationCommands.Keys)
                battlestationChoices.Add(key);

            /*
             * The GrammarBuilder defines what the requisted "flow" is of the possible commands.
             * You can insert plain text, or a Choices object with all our values in it, in our case our commands
             * We also need to pass in our Culture so that it knows what language we're talking
             */
            GrammarBuilder initCmdBuilder = new GrammarBuilder { Culture = info.Culture };
            initCmdBuilder.Append(battlestationChoices);
            initCmdBuilder.Append("battlestation");

            // Create our speech grammar
            Grammar battlestationGrammar = new Grammar(initCmdBuilder) { Name = "BattlestationGrammar" };
            #endregion Global speech CMD grammar

            #region Drone CMD grammar
            // Add our drone commands as "Choices"
            Choices droneCmds = new Choices();
            foreach (string key in _droneCommands.Keys)
                droneCmds.Add(key);

            /*
             * The GrammarBuilder defines what the requisted "flow" is of the possible commands.
             * You can insert plain text, or a Choices object with all our values in it, in our case our commands
             * We also need to pass in our Culture so that it knows what language we're talking
             */
            GrammarBuilder droneCmdBuilder = new GrammarBuilder { Culture = info.Culture };
            droneCmdBuilder.Append("Drone");
            droneCmdBuilder.Append(droneCmds);

            // Create our speech grammar
            Grammar droneCmdGrammar = new Grammar(droneCmdBuilder) { Name = "DroneGrammar" };
            #endregion Drone CMD grammar

            // Prevent crashes
            if (_currentSensor == null || _recognizer == null)
                return;

            // Load grammer into our recognizer
            _recognizer.LoadGrammar(battlestationGrammar);
            _recognizer.LoadGrammar(droneCmdGrammar);

            // Hook into speech events
            _recognizer.SpeechRecognized += OnCommandRecognizedHandler;
            _recognizer.SpeechRecognitionRejected += OnCommandRejectedHandler;

            // Get the kinect audio stream
            _audioSource = _currentSensor.AudioSource;

            // Set the beamangle
            _audioSource.BeamAngleMode = BeamAngleMode.Adaptive;

            // Start the kinect audio
            Stream kinectStream = _audioSource.Start();

            // Assign the stream to the recognizer along with FormatInfo
            _recognizer.SetInputToAudioStream(kinectStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));

            // Start recognizingand make sure to tell that the RecognizeMode is Multiple or it will stop after the first recognition
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        /// <summary>
        /// Command recognized
        /// </summary>
        private void OnCommandRecognizedHandler(object sender, SpeechRecognizedEventArgs e)
        {
            TimeSpan interval = DateTime.Now.Subtract(_lastCommand);

            if (interval.TotalSeconds < _delayInSeconds || e.Result == null) return;

            // Be sure that the result is confident enough
            if (e.Result.Confidence < 0.85f)
            {
                //Narrator.Speak("Can you repeat that please?");
                return;
            }

            if (e.Result.Grammar == null) return;

            // Execute command based on grammar name
            switch (e.Result.Grammar.Name)
            {
                case "BattlestationGrammar":
                    ExecuteBattlestationCommand(e.Result);
                    break;
                case "DroneGrammar":
                    ExecuteDroneVoiceCommand(e.Result);
                    break;
            }
        }

        /// <summary>
        /// Command rejected
        /// </summary>
        private void OnCommandRejectedHandler(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            WriteLog("Unkown command");
        }

        /// <summary>
        /// Get the first RecognizerInfo-object that is a Kinect & has the English pack
        /// </summary>
        private RecognizerInfo GetKinectRecognizer()
        {
            /* Create a function that checks if the additioninfo contains a key called "Kinect" and if it's true.
             * Also check if the culture is en-US so that we're using the English pack
            */
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);

                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) &&
                       "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };

            return SpeechRecognitionEngine.InstalledRecognizers().FirstOrDefault(matchingFunc);
        }
        #endregion Kinect Speech

        #region Kinect camera
        /// <summary>
        /// Process color data & skeleton data
        /// </summary>
        private void OnAllFramesReadyHandler(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                    return;

                // Visualize camera
                VisualizeCamera(colorFrame);

                using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (skeletonFrame == null) return;

                    // Copy skeletons
                    skeletonFrame.CopySkeletonDataTo(_skeletons);

                    // Wipe skeleton canvas
                    KinectSkeleton.Children.Clear();

                    // Visualize all skeletons
                    foreach (Skeleton skel in _skeletons)
                        VisualizeSkeleton(skel);

                    AnalyzeSkeleton(
                        _skeletons.OrderByDescending(skel => skel.Position.Z)
                            .FirstOrDefault(skel => skel.TrackingState == SkeletonTrackingState.Tracked));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skeleton"></param>
        private void AnalyzeSkeleton(Skeleton skeleton)
        {
            if (_drone == null || _drone.Connection != DroneConnection.Connected || _drone.IsFlying == false) return;

            DroneFlyingGesture trackedGestures = GestureMonitor.Analyse(skeleton);

            _drone.Control(trackedGestures);

            // Temp flush to log
            WriteLog(trackedGestures.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skel"></param>
        private void VisualizeSkeleton(Skeleton skel)
        {
            if (skel.TrackingState != SkeletonTrackingState.Tracked) return;

            // Draw points
            foreach (Joint joint in skel.Joints)
            {
                // Draw all the body joints
                switch (joint.JointType)
                {
                    case JointType.HandLeft:
                    case JointType.HandRight:
                        DrawJoint(joint, 20, Brushes.Yellow, 2, Brushes.White);
                        break;
                    case JointType.ShoulderLeft:
                    case JointType.ShoulderRight:
                    case JointType.HipCenter:
                        DrawJoint(joint, 20, Brushes.YellowGreen, 2, Brushes.White);
                        break;
                    default:
                        break;
                }
            }


        }

        /// <summary>
        /// Draws a body joint
        /// </summary>
        /// <param name="joint">Joint of the body</param>
        /// <param name="radius">Circle radius</param>
        /// <param name="fill">Fill color</param>
        /// <param name="borderWidth">Thickness of the border</param>
        /// <param name="border">Color of the boder</param>
        private void DrawJoint(Joint joint, double radius, SolidColorBrush fill, double borderWidth, SolidColorBrush border)
        {
            if (joint.TrackingState != JointTrackingState.Tracked) return;

            // Map the CameraPoint to ColorSpace so they match
            ColorImagePoint colorPoint = _currentSensor.CoordinateMapper.MapSkeletonPointToColorPoint(joint.Position, ColorImageFormat.RgbResolution640x480Fps30);

            // Avoid exceptions based on bad tracking
            if (float.IsInfinity(colorPoint.X) || float.IsInfinity(colorPoint.X)) return;

            // Create the UI element based on the parameters
            Ellipse el = new Ellipse();
            el.Fill = fill;
            el.Stroke = border;
            el.StrokeThickness = borderWidth;
            el.Width = el.Height = radius;

            // Add the Ellipse to the canvas
            KinectSkeleton.Children.Add(el);

            // Allign ellipse on canvas (Divide by 2 because image is only 50% of original size)
            Canvas.SetLeft(el, colorPoint.X);
            Canvas.SetTop(el, colorPoint.Y);
        }

        /// <summary>
        /// Visualize the camera data
        /// </summary>
        private void VisualizeCamera(ColorImageFrame colorFrame)
        {
            if (colorFrame == null)
                return;

            // Initialize variables
            if (_kinectPixelData.Length == 0)
            {
                // Create buffer
                _kinectPixelData = new byte[colorFrame.PixelDataLength];

                // Create output rep
                _cameraVision = new WriteableBitmap(colorFrame.Width,
                                                    colorFrame.Height,

                                                    // DPI
                                                    96, 96,

                                                    // Current pixel format
                                                    PixelFormats.Bgr32,

                                                    // Bitmap palette
                                                    null);

                // Hook image to Image-control
                KinectImage.Source = _cameraVision;
            }

            // Copy data from frame to buffer
            colorFrame.CopyPixelDataTo(_kinectPixelData);

            // Update bitmap
            _cameraVision.WritePixels(

                // Image size
                new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),

                // Buffer
                _kinectPixelData,

                // Stride
                colorFrame.Width * colorFrame.BytesPerPixel,

                // Buffer offset
                0);
        }
        #endregion Kinect camera

        private bool _commanderPresent = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recognitionResult"></param>
        private void ExecuteBattlestationCommand(RecognitionResult recognitionResult)
        {
            BattlestationVoiceCommand cmd = GetBattlestationVoiceCommand(recognitionResult);

            if (cmd == BattlestationVoiceCommand.Hello)
            {
                string greetings = string.Empty;

                // Greet commander based on current time
                if (DateTime.Now.Hour > 5 && DateTime.Now.Hour <= 12)
                {
                    greetings = "Good morning commander!";
                }
                else if (DateTime.Now.Hour > 12 && DateTime.Now.Hour <= 22)
                {
                    greetings = "Good evening commander!";
                }
                else
                    greetings = "Goodnight commander!";

                Narrator.Speak(greetings);
                Narrator.Speak("We are ready for lift-off");

                _commanderPresent = true;
            }
            else _commanderPresent = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recogResult"></param>
        /// <returns></returns>
        private BattlestationVoiceCommand GetBattlestationVoiceCommand(RecognitionResult recogResult)
        {
            if (recogResult == null || string.IsNullOrEmpty(recogResult.Text))
                return BattlestationVoiceCommand.Unknown;

            // Seperate 'Drone' from command grammar
            Match m = Regex.Match(recogResult.Text, "^(.*)battlestation$");

            // Check if it matches
            if (m.Success)
            {
                // Get command from object
                KeyValuePair<string, object> cmd =
                   _battlestationCommands.FirstOrDefault(action => action.Key.ToLower() == m.Groups[1].ToString().Trim().ToLower());

                if (cmd.Value != null)
                {
                    return (BattlestationVoiceCommand)cmd.Value;
                }
                return BattlestationVoiceCommand.Unknown;
            }
            else return BattlestationVoiceCommand.Unknown;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recognitionResult"></param>
        private void ExecuteDroneVoiceCommand(RecognitionResult recognitionResult)
        {
            if (_commanderPresent == true)
            {
                // Retrieve the DroneState from the recognized result
                DroneVoiceCommand droneCmd = GetDroneCommand(recognitionResult);

                // Log state
                WriteLog("Command '" + droneCmd.GetDescription() + "' recognized.");

                switch (droneCmd)
                {
                    case DroneVoiceCommand.Land:
                        Narrator.Speak("Landing drone");
                        _drone.ChangeFlyingState(DroneFlyingAction.Land);
                        break;
                    case DroneVoiceCommand.TakeOff:
                        Narrator.Speak("Going airborne");
                        _drone.ChangeFlyingState(DroneFlyingAction.TakeOff);
                        break;
                    case DroneVoiceCommand.EmergencyLanding:
                        Narrator.Speak("Mayday");
                        _drone.ChangeFlyingState(DroneFlyingAction.Emergency);
                        break;
                    case DroneVoiceCommand.ChangeVideoChannel:
                        _drone.ChangeVideoChannel();
                        Narrator.Speak("Channel changed");
                        break;
                    case DroneVoiceCommand.ShootLeft:
                        Narrator.Speak("Firing left missle");
                        _drone.PerformLEDAnimation(LedAnimationType.LeftMissile);
                        break;
                    case DroneVoiceCommand.ShootRight:
                        Narrator.Speak("Firing right missle");
                        _drone.PerformLEDAnimation(LedAnimationType.RightMissile);
                        break;
                    case DroneVoiceCommand.FrontFlip:
                        Narrator.Speak("Performing front flip");
                        _drone.PerformFlightAnimation(FlightAnimationType.FlipAhead);
                        break;
                    case DroneVoiceCommand.BackFlip:
                        Narrator.Speak("Performing back flip");
                        _drone.PerformFlightAnimation(FlightAnimationType.FlipBehind);
                        break;
                    case DroneVoiceCommand.SideFlip:
                        Narrator.Speak("Performing side flip");
                        _drone.PerformFlightAnimation(FlightAnimationType.FlipLeft);
                        break;
                    case DroneVoiceCommand.GoCrazy:
                        Narrator.Speak("Don't be crazy, you fool!");
                        //_drone.PerformRandomFlightAnimation();
                        break;
                    default:
                        break;
                }
            }
            else
                Narrator.Speak("Identify yourself");
        }

        /// <summary>
        /// Retrieve the requested Drone actions
        /// </summary>
        private DroneVoiceCommand GetDroneCommand(RecognitionResult recogResult)
        {
            if (recogResult == null || string.IsNullOrEmpty(recogResult.Text))
                return DroneVoiceCommand.Unknown;

            // Seperate 'Drone' from command grammar
            Match m = Regex.Match(recogResult.Text, "^Drone (.*)$");

            // Check if it matches
            if (m.Success)
            {
                // Get command from object
                KeyValuePair<string, object> cmd =
                   _droneCommands.FirstOrDefault(action => action.Key.ToLower() == m.Groups[1].ToString().ToLower());

                if (cmd.Value != null)
                {
                    return (DroneVoiceCommand)cmd.Value;
                }
                return DroneVoiceCommand.Unknown;
            }
            else return DroneVoiceCommand.Unknown;
        }

        #region Drone Global
        /// <summary>
        /// Initialisation of the drone
        /// </summary>
        private void InitializeDrone()
        {
            // Create new Drone instance
            _drone = new Drone();

            // Connect to the drone
            _drone.Connect();

            // Update Connection
            DroneConnection = DroneConnection.Connected;

            // Link event handlers
            _drone.VideoFrameAcquired += OnVideoFrameAcquired;
            _drone.DroneInfoAcquired += OnDroneInfoAcquired;
        }

        /// <summary>
        /// Process Drone information
        /// </summary>
        public void OnDroneInfoAcquired(NavigationData data)
        {
            DroneState = data.State;
            DroneBatteryLevel = (int)data.Battery.Percentage;
        }
        #endregion Drone Global

        #region Drone Video
        /// <summary>
        /// New Drone output infromation is available
        /// </summary>
        /// <param name="height">Heigth of the drone output</param>
        /// <param name="width">Width of the drone output</param>
        /// <param name="imageBytes">Bytes containing output</param>
        private void OnVideoFrameAcquired(int height, int width, byte[] imageBytes)
        {
            Dispatcher.BeginInvoke(
                   (Action)delegate
                   {
                       DrawDroneOutput(height, width, imageBytes);
                   });
        }

        /// <summary>
        /// Visualise Drone output
        /// </summary>
        /// <param name="height">Heigth of the drone output</param>
        /// <param name="width">Width of the drone output</param>
        /// <param name="imageBytes">Bytes containing output</param>
        private void DrawDroneOutput(int height, int width, byte[] imageBytes)
        {
            if (_droneVideo == null)
            {
                _droneVideo = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);

                DroneImage.Source = _droneVideo;
            }

            // Create image rect
            Int32Rect imageSize = new Int32Rect(0, 0, width, height);

            // Write pixels
            _droneVideo.WritePixels(imageSize, imageBytes, width * 3, 0);
        }
        #endregion Drone Video



        #region UI Properties
        private KinectStatus _kinectStatus = KinectStatus.Disconnected;
        public KinectStatus KinectStatus
        {
            get { return _kinectStatus; }
            set
            {
                if (_kinectStatus != value)
                {
                    _kinectStatus = value;
                    OnPropertyChanged("KinectStatus");
                }
            }
        }

        private DroneConnection _droneConnection = DroneConnection.NotConnected;
        public DroneConnection DroneConnection
        {
            get { return _droneConnection; }
            set
            {
                if (_droneConnection != value)
                {
                    _droneConnection = value;
                    OnPropertyChanged("DroneConnection");
                }
            }
        }

        private NavigationState _droneState = NavigationState.Unknown;
        public NavigationState DroneState
        {
            get { return _droneState; }
            set
            {
                if (_droneState != value)
                {
                    _droneState = value;
                    OnPropertyChanged("DroneState");
                }
            }
        }

        private int _droneBatteryLevel = 0;
        public int DroneBatteryLevel
        {
            get { return _droneBatteryLevel; }
            set
            {
                if (_droneBatteryLevel != value)
                {
                    _droneBatteryLevel = value;
                    OnPropertyChanged("DroneBatteryLevel");
                }
            }
        }

        private string _log = "> Welcome!";
        public string Log
        {
            get { return _log; }
            set
            {
                if (_log != value)
                {
                    _log = value;
                    OnPropertyChanged("Log");
                }
            }
        }
        #endregion UI Properties

        #region Internal Methods/Events & UI Properties
        private void WriteLog(string output)
        {
            Log = "> " + output;
        }
        #endregion Internal Methods

        #region Internal events
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion Internal Events
    }
}
