using System.IO;
using System.Speech.AudioFormat;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;

namespace K4W.KinectDrone.Core
{
    public class KinectSpeechManager
    {
        private KinectSensor _sensor = null;
        private Dictionary<string, object> _vocabulary;
        private string _cmdPrefix;

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

        public KinectSpeechManager(KinectSensor sensor)
        {
            _sensor = sensor;
        }

        public void Initialize(string cmdPrefix, Dictionary<string, object> vocabulary)
        {
            // Check if vocabulary is specifief
            if (vocabulary == null || vocabulary.Count == 0)
                throw new ArgumentException("A vocabulary is required.");

            // Copy vocabulary
            _vocabulary = vocabulary;

            // Check sensor state
            if (_sensor.Status != KinectStatus.Connected)
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

            // Add our commands as "Choices"
            Choices cmds = new Choices();
            foreach (string key in _vocabulary.Keys)
                cmds.Add(key);

            /*
             * The GrammarBuilder defines what the requisted "flow" is of the possible commands.
             * You can insert plain text, or a Choices object with all our values in it, in our case our commands
             * We also need to pass in our Culture so that it knows what language we're talking
             */
            GrammarBuilder cmdBuilder = new GrammarBuilder { Culture = info.Culture };
            cmdBuilder.Append(cmdPrefix);
            cmdBuilder.Append(cmds);

            // Create our speech grammar
            Grammar cmdGrammar = new Grammar(cmdBuilder);

            // Prevent crashes
            if(_sensor == null ||_recognizer== null)
                return;
            
            // Load grammer into our recognizer
            _recognizer.LoadGrammar(cmdGrammar);

            // Hook into speech events
            _recognizer.SpeechRecognized += OnCommandRecognized;
            _recognizer.SpeechRecognitionRejected += _recognizer_SpeechRecognitionRejected;

            // Get the kinect audio stream
            _audioSource = _sensor.AudioSource;

            // Set the beamangle
            _audioSource.BeamAngleMode = BeamAngleMode.Adaptive;
            
            // Start the kinect audio
            Stream kinectStream = _audioSource.Start();

            // Assign the stream to the recognizer along with FormatInfo
            _recognizer.SetInputToAudioStream(kinectStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));

            // Start recognizingand make sure to tell that the RecognizeMode is Multiple or it will stop after the first recognition
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        void _recognizer_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            CommandRecognized("rejected");
        }

        private void OnCommandRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            TimeSpan interval = DateTime.Now.Subtract(_lastCommand);

            if (interval.TotalSeconds < _delayInSeconds)
                return;

            if (e.Result.Confidence < 0.80f)
            {
                if (e.Result.Alternates.Count > 0)
                {
                    // Print the alternatives
                    Console.WriteLine("Alternates available: " + e.Result.Alternates.Count);
                    foreach (RecognizedPhrase alternate in e.Result.Alternates)
                    {
                        Console.WriteLine("Alternate: " + alternate.Text + ", " + alternate.Confidence);
                    }
                }
                return;
            }

            // Bubble event
            CommandRecognized(e.Result.Alternates[0].Text);
        }
    }
}
