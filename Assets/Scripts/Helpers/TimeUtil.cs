using System;
using UnityEngine;

namespace VARLab.DLX
{
    public static class TimeUtil
    {
        /// <summary>
        ///     Generates a timestamp string in the format HH:MM:SS for the time 
        ///     since the application launched.
        /// </summary>
        /// <returns> A string representing the current timestamp. </returns>
        public static string Timestamp()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(Time.time);
            return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
    }
}
