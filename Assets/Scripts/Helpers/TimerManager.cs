using System;
using System.Diagnostics;
using UnityEngine;

namespace VARLab.DLX
{
    /// <summary>
    ///     Singleton class that manages the timer for the game.
    ///     This timer is used to track the time the player has spent in the game. 
    ///     When the game is loaded, the timers value are set in <see cref="SaveDataSupport.LoadTimer"/>
    /// </summary>
    public class TimerManager : MonoBehaviour
    {
        /// <summary> The current instance of the TimerManager </summary>
        public static TimerManager Instance { get; private set; }

        /// <summary> The stopwatch used to track the time </summary>
        public Stopwatch Timer { get; private set; }

        /// <summary> The stop watch class is basically read only so we must manually track an offset to add on time from save. </summary>
        public TimeSpan Offset;

        // Start is called before the first frame update
        private void Awake()
        {
            // Check to see if the current instance is non-null, according to Unity.
            // It must also not be this same object (which is also basically impossible)
            if (Instance == null)
            {
                Instance = this;
                Timer = new Stopwatch();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            Offset = new TimeSpan(0);

            // Temporary
            StartTimers();
        }

        /// <summary>
        ///     This method is used to get the current time span of the timer in a string format
        /// </summary>
        /// <returns>Returns span of timer in string format</returns>
        public string GetElapsedTime()
        {
            TimeSpan ts = GetTimeSpan();
            string s = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
            return s;
        }

        /// <summary>
        ///     Starts the timer
        /// </summary>
        public void StartTimers()
        {
            Timer.Start();
        }

        /// <summary>
        ///    Pauses the timer
        /// </summary>
        public void PauseTimers()
        {
            Timer.Stop();
        }

        /// <summary>
        ///     Restarts the timer
        /// </summary>
        public void RestartTimers()
        {
            Timer.Restart();
        }


        /// <summary>
        ///     This method is used to get the current time span of the timer
        ///     Use: This will allow me to use the Hours/Minutes/Seconds properties of the TimeSpan class to format the time string easily in the progress bar
        /// </summary>
        /// <returns>A TimeSpan struct</returns>
        public TimeSpan GetTimeSpan()
        {
            return TimerManager.Instance.Timer.Elapsed + Offset;
        }

        /// <summary>
        ///     This method is used to convert a time span to a string
        ///     Note: If the time reaches 1hr, seconds will not be converted. Ex. 1hr 2mins 3secs will be "1hr 2mins"
        /// </summary>
        /// <param name="timeSpan">The timespan that will be converted.</param>
        /// <returns>A formatted string of the time with the wording of hr(s)/min(s)/sec(s)</returns>
        public string ConvertTimeSpanToString()
        {
            string returnString = "";
            TimeSpan timeSpan = GetTimeSpan();

            //Formatting the time span to be displayed in the progress bar
            if (timeSpan.Hours > 0)
                returnString += (returnString != "" ? " " : "") + timeSpan.Hours.ToString() + " h";

            if (timeSpan.Minutes > 0)
                returnString += (returnString != "" ? " " : "") + timeSpan.Minutes.ToString() + " m";

            if (timeSpan.Seconds > 0 && timeSpan.Hours <= 0)
                returnString += (returnString != "" ? " " : "") + timeSpan.Seconds.ToString() + " s";

            return returnString;
        }
    }
}
