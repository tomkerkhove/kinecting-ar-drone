using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace K4W.KinectDrone.Core.Speech
{
    public class Narrator
    {
        /// <summary>
        /// Synth instance
        /// </summary>
        private static readonly SpeechSynthesizer _speaker = new SpeechSynthesizer();

        /// <summary>
        /// Speak a certain phrase
        /// </summary>
        public static void Speak(string phrase)
        {
            _speaker.SpeakAsync(phrase);
        }
    }
}
