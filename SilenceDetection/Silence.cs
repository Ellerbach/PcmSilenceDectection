using System;

namespace SilenceDetection
{
    /// <summary>
    /// Class containing silence information
    /// </summary>
    public class Silence
    {
        /// <summary>
        /// Start time for the silence
        /// </summary>
        public TimeSpan Start { get; set; }
        /// <summary>
        /// duration time for the silence
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// Index in the raw byte array of the start of the silence
        /// </summary>
        public int IndexStart { get; set; }
        /// <summary>
        /// Index in the raw byte array of the end of the silence
        /// </summary>
        public int IndexEnd { get; set; }
    }
}
