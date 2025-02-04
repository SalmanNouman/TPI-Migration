using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.DLX;

namespace Tests
{
    public class TimerManagerTests
    {
        public GameObject Timer;


        /// <summary>
        /// Runs before each test to create a fresh TimerManager instance.
        /// It ensures the timer starts clean and resets any offsets.
        /// </summary>
        [SetUp]
        public void RunBeforeEveryTest()
        {
            Timer = new GameObject();
            Timer.AddComponent<TimerManager>();

            // Reset the singleton instance's offset before every test
            TimerManager.Instance.Offset = TimeSpan.Zero;
            TimerManager.Instance.RestartTimers();
        }

        
        /// <summary>
        ///     This test checks if the timer starts at 00:00:00 when restarted.
        /// </summary>
        /// <returns>Passes if the elapsed time is exactly "00:00:00". </returns>
        [UnityTest]
        public IEnumerator TimerStartsAtZero()
        {
            // Arrange
            string expectedResult = "00:00:00";

            // Act 
            TimerManager.Instance.RestartTimers();
            var result = TimerManager.Instance.GetElapsedTime();
            yield return null;

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        /// <summary>
        ///     This test checks if the timer is running properly after starting.
        /// </summary>
        /// <returns>Passes if the timer is at Least 2 seconds after waiting.</returns>
        [UnityTest]
        public IEnumerator TimerIsRunning()
        {
            //Arrange
            var expectedResult = 2.0;

            //Act
            TimerManager.Instance.StartTimers();
            yield return new WaitForSeconds(2f);

            //Assert
            Assert.GreaterOrEqual(TimerManager.Instance.GetTimeSpan().TotalSeconds, expectedResult);
            Debug.Log(TimerManager.Instance.GetTimeSpan().TotalSeconds);
        }

        /// <summary>
        ///     This test verifies if the timer resets correctly after running.
        /// </summary>
        /// <returns>Passes if the timer resets back to zero after being restarted.</returns>
        [UnityTest]
        public IEnumerator TimerIsResetting()
        {
            //Arrange
            var expectedResult = 2.0;
            var expectedResultTwo = 0.01;

            //Act
            TimerManager.Instance.StartTimers();
            yield return new WaitForSeconds(2f);
            Assert.GreaterOrEqual(TimerManager.Instance.GetTimeSpan().TotalSeconds, expectedResult);
            TimerManager.Instance.RestartTimers();
            Debug.Log(TimerManager.Instance.GetTimeSpan().TotalSeconds);
            yield return null; //allows unity one frame to process updates
            Debug.Log(TimerManager.Instance.GetTimeSpan().TotalSeconds);

            //Assert
            Assert.Less(TimerManager.Instance.GetTimeSpan().TotalSeconds, expectedResultTwo);
            Debug.Log(TimerManager.Instance.GetTimeSpan().TotalSeconds);
        }

        /// <summary>
        ///     This test checks if an offset is correctly applied to the timer.
        ///     Example: If an offset of 3 seconds is set, and the timer runs for 1 seconds, the total should be 4 seconds.
        /// </summary>
        /// <returns>Passes if the elapsed time is within the expected range.</returns>
        [UnityTest]
        public IEnumerator TimersOffset()
        {
            //Arrange
            var expectedResult = 4;
            TimeSpan offsetTime = new(0, 0, 3);
            TimerManager.Instance.Offset = offsetTime;

            //Act
            TimerManager.Instance.RestartTimers();
            yield return new WaitForSeconds(1f);
            Debug.Log(TimerManager.Instance.GetTimeSpan().TotalSeconds);

            //Assert
            Assert.GreaterOrEqual(TimerManager.Instance.GetTimeSpan().TotalSeconds, expectedResult);
            Debug.Log(TimerManager.Instance.GetTimeSpan().TotalSeconds);
        }
    }
}
