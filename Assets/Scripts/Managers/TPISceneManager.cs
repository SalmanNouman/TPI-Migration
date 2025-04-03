using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace VARLab.DLX
{
    /// <summary>
    ///     Manages scene operations and handles scene restart functionality.
    /// </summary>
    public class TPISceneManager : MonoBehaviour
    {
        #region Fields
        // Indicates whether the scene load operation has completed.
        public static bool LoadCompleted = false;

        // Indicates whether the cloud save data has been deleted.
        public static bool CloudSaveDeleted = false;

        #endregion

        #region Events

        /// <summary>
        ///     Event triggered just before the scene is restarted.
        /// </summary>
        /// <remarks>
        ///     Inspector connections:
        ///     - Called by <see cref="SaveDataSupport.OnLoadRestart"/> after save file deletion
        /// </remarks>
        public UnityEvent OnSceneRestart;

        #endregion

        #region Properties

        /// <summary>
        ///     Singleton instance of the TPISceneManager.
        /// </summary>
        public static TPISceneManager Instance { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes the singleton instance and event to prevent null reference exceptions.
        /// </summary>
        private void Awake()
        {
            // Set up singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Initialize event
            OnSceneRestart ??= new();
        }

        /// <summary>
        ///     Initiates the scene restart process.
        /// </summary>
        public void RestartScene()
        {
            StartCoroutine(RestartSceneCoroutine());
        }

        /// <summary>
        ///     Handles the scene restart process by resetting static variables and reloading the scene.
        /// </summary>
        private IEnumerator RestartSceneCoroutine()
        {
            // TODO: Reset intro task flags
            
            // Invoke the restart event
            OnSceneRestart?.Invoke();

            LoadCompleted = false;

            // Load new scene
            // Note: Until the Fade system is implemented, the code after LoadSceneAsync may not execute as the object will be destroyed during scene transition.
            var loading = SceneManager.LoadSceneAsync("Main Scene");
            while (!loading.isDone) yield return null;
            LoadCompleted = true;
        }

        #endregion
    }
}