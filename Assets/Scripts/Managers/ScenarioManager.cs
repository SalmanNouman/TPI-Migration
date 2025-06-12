using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using VARLab.DLX;

namespace VARLab.DLX
{
    /// <summary>
    ///     Manages scenario operations
    /// </summary>
    /// <remarks>
    ///     TPI-343 implementation:
    ///     - Loads random scenario when new game starts (no save file or deleted save file)
    ///     - Saves scenario name when scenario loading completes
    ///     - Updates inspectables according to saved scenario when continuing game
    /// </remarks>
    public class ScenarioManager : MonoBehaviour
    {
        #region Fields

        /// <summary>
        ///     List of available scenarios that can be assigned in the Inspector
        /// </summary>
        [SerializeField, Tooltip("List of available Scenarios - drag and drop ScenarioData assets here")]
        private List<ScenarioData> availableScenarios = new List<ScenarioData>();

        /// <summary>
        ///     Name of the currently active scenario
        /// </summary>
        [SerializeField] private string currentScenarioName;

        /// <summary>
        ///     Dictionary storing all InspectableObjects in the scene, organized by POI
        /// </summary>
        private Dictionary<string, List<InspectableObject>> poiAndInspectables;

        #endregion

        #region Properties

        /// <summary>
        ///     Returns the name of the current scenario.
        /// </summary>
        public string CurrentScenarioName => currentScenarioName;

        /// <summary>
        ///     Singleton instance of the ScenarioManager.
        /// </summary>
        public static ScenarioManager Instance { get; private set; }

        #endregion

        #region Events

        /// <summary>
        ///     Event triggered when scenario application to the scene is completed.
        /// </summary>
        public UnityEvent OnScenarioApplied;

        /// <summary>
        ///     Event triggered when a scenario is loaded in new simulations.
        /// </summary>
        /// <remarks>
        ///     Inspector connections:
        ///     - <see cref="CustomSaveHandler.SaveNewScenario(string)"/> (TPI-343: Save scenario for new games)
        /// </remarks>
        public UnityEvent<string> OnNewScenarioLoaded;

        /// <summary>
        ///     Event triggered when saved scenario loading is completed.
        /// </summary>
        /// <remarks>
        ///     Inspector connections:
        ///     - <see cref="CustomSaveHandler.InvokeLoadPhotos"/> (Restore gallery photos after scenario loading)
        /// </remarks>
        public UnityEvent OnSavedScenarioLoaded;

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes singleton pattern and events to prevent null reference exceptions.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            OnScenarioApplied ??= new();
            OnNewScenarioLoaded ??= new();
            OnSavedScenarioLoaded ??= new();
        }

        /// <summary>
        ///     Collects InspectableObjects
        /// </summary>
        private void Start()
        {
            CollectInspectableObjects();
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Editor-only: Handles development/testing scenario switching shortcuts.
        /// </summary>
        private void Update()
        {
            HandleEditorScenarioSwitching();
        }

        /// <summary>
        ///     Editor-only: Handles scenario switching via keyboard shortcuts.
        ///     Shift + 1-9: Switch to scenario at index (0-8)
        /// </summary>
        private void HandleEditorScenarioSwitching()
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return;

            if (availableScenarios == null || availableScenarios.Count == 0)
                return;

            int targetIndex = -1;

            // Check number keys 1-9 (maps to index 0-8)
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i - 1)))
                {
                    targetIndex = i - 1;
                    break;
                }
            }

            // Switch scenario if valid index and scenario exists
            if (targetIndex >= 0 && targetIndex < availableScenarios.Count)
            {
                SwitchToScenarioByIndex(targetIndex);
            }
        }

        /// <summary>
        ///     Editor-only: Immediately switches to a scenario by index.
        /// </summary>
        /// <param name="index">Index of the scenario in the availableScenarios list</param>
        private void SwitchToScenarioByIndex(int index)
        {
            if (index < 0 || index >= availableScenarios.Count)
            {
                Debug.LogWarning($"ScenarioManager [Editor]: Invalid scenario index {index}. Available scenarios: {availableScenarios.Count}");
                return;
            }

            ScenarioData scenario = availableScenarios[index];
            currentScenarioName = scenario.ScenarioName;
            
            Debug.Log($"ScenarioManager [Editor]: Switched to scenario '{currentScenarioName}'.");
            
            // Apply the scenario
            ApplyScenario(scenario);
        }
#endif

        /// <summary>
        ///     Collects all InspectableObjects in the scene, organized by POI.
        /// </summary>
        public void CollectInspectableObjects()
        {
            poiAndInspectables = new Dictionary<string, List<InspectableObject>>();

            // Find all InspectableObjects in the scene
            InspectableObject[] inspectableObjects = FindObjectsByType<InspectableObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            // Group by POI
            foreach (InspectableObject inspectable in inspectableObjects)
            {
                string location = inspectable.Location.ToString();

                if (!poiAndInspectables.ContainsKey(location))
                {
                    poiAndInspectables[location] = new List<InspectableObject>();
                }

                // Generate ObjectId if missing
                if (string.IsNullOrEmpty(inspectable.ObjectId))
                {
                    inspectable.GeneratedId();
                }

                poiAndInspectables[location].Add(inspectable);
            }

            Debug.Log($"ScenarioManager: Collected {inspectableObjects.Length} InspectableObjects from {poiAndInspectables.Count} POIs.");
        }

        /// <summary>
        ///     Randomly selects and applies one of the available scenarios.
        /// </summary>
        /// <remarks>
        ///     Inspector connections:
        ///     - Connected from: <see cref="CustomSaveHandler.OnFreshLoad"/> event
        /// </remarks>
        public void LoadRandomScenario()
        {
            if (availableScenarios == null || availableScenarios.Count == 0)
            {
                Debug.LogError("ScenarioManager: No available scenarios found.");
                return;
            }

            // Select random scenario
            int randomIndex = Random.Range(0, availableScenarios.Count);
            ScenarioData selectedScenario = availableScenarios[randomIndex];
            
            currentScenarioName = selectedScenario.ScenarioName;
            
            Debug.Log($"ScenarioManager: Selected random scenario '{currentScenarioName}'.");
            
            // Apply the scenario
            ApplyScenario(selectedScenario);
            
            // Invoke new scenario loaded event
            OnNewScenarioLoaded?.Invoke(currentScenarioName);
        }

        /// <summary>
        ///     Loads and applies a scenario by its specific name.
        /// </summary>
        /// <remarks>
        ///     Inspector connections:
        ///     - Connected from: <see cref="CustomSaveHandler.LoadSavedScenario"/> event
        /// </remarks>
        /// <param name="scenarioName">Name of the scenario to load</param>
        public void LoadScenarioByName(string scenarioName)
        {
            if (string.IsNullOrEmpty(scenarioName))
            {
                Debug.LogWarning("ScenarioManager: Scenario name is empty. Loading random scenario.");
                LoadRandomScenario();
                return;
            }

            // Find the scenario
            ScenarioData scenario = availableScenarios?.Find(s => s.ScenarioName == scenarioName);
            
            if (scenario == null)
            {
                Debug.LogError($"ScenarioManager: Could not find scenario '{scenarioName}' in assigned scenarios. Loading random scenario.");
                LoadRandomScenario();
                return;
            }

            currentScenarioName = scenarioName;
            
            Debug.Log($"ScenarioManager: Loaded saved scenario '{currentScenarioName}'.");
            
            // Apply the scenario
            ApplyScenario(scenario);
            
            // Invoke saved scenario loaded event (for gallery loading timing adjustment)
            OnSavedScenarioLoaded?.Invoke();
        }

        /// <summary>
        ///     Applies scenario data to InspectableObjects in the scene by setting their states.
        /// </summary>
        /// <param name="scenario">Scenario data to apply</param>
        private void ApplyScenario(ScenarioData scenario)
        {
            if (scenario == null || scenario.InspectablesAndStates == null)
            {
                Debug.LogError("ScenarioManager: Scenario data is invalid.");
                return;
            }

            int appliedCount = 0;

            foreach (var inspectableData in scenario.InspectablesAndStates)
            {
                InspectableObject targetObject = FindInspectableByID(inspectableData.InspectableID);
                
                if (targetObject != null)
                {
                    ApplyStateToInspectable(targetObject, inspectableData.InspectableState);
                    appliedCount++;
                }
                else
                {
                    Debug.LogWarning($"ScenarioManager: Could not find InspectableObject with ID '{inspectableData.InspectableID}'.");
                }
            }

            Debug.Log($"ScenarioManager: Applied scenario to {appliedCount} InspectableObjects.");
            OnScenarioApplied?.Invoke();
        }

        /// <summary>
        ///     Finds an InspectableObject by its ObjectID.
        /// </summary>
        /// <param name="objectId">ID of the InspectableObject to find</param>
        /// <returns>Found InspectableObject, or null if not found</returns>
        private InspectableObject FindInspectableByID(string objectId)
        {
            if (poiAndInspectables == null) return null;

            foreach (var kvp in poiAndInspectables)
            {
                InspectableObject obj = kvp.Value.Find(inspectable => inspectable.ObjectId == objectId);
                if (obj != null) return obj;
            }

            return null;
        }

        /// <summary>
        ///     Applies a specific state to an InspectableObject.
        /// </summary>
        /// <param name="inspectable">InspectableObject to apply the state to</param>
        /// <param name="stateName">Name of the state to apply</param>
        private void ApplyStateToInspectable(InspectableObject inspectable, string stateName)
        {
            if (inspectable == null || inspectable.States == null)
            {
                Debug.LogWarning("ScenarioManager: InspectableObject or States is null.");
                return;
            }

            // Find the state that matches the state name
            State targetState = null;
            foreach (var state in inspectable.States)
            {
                string currentStateName = state.InspectableGameObject != null ? 
                    state.InspectableGameObject.name : state.Compliancy.ToString();

                if (currentStateName == stateName)
                {
                    targetState = state;
                    break;
                }
            }

            if (targetState == null)
            {
                Debug.LogWarning($"ScenarioManager: Could not find state '{stateName}' in '{inspectable.Name}'.");
                return;
            }

            // Deactivate all state GameObjects and activate only the selected state
            foreach (var state in inspectable.States)
            {
                if (state.InspectableGameObject != null)
                {
                    state.InspectableGameObject.SetActive(state == targetState);
                }
            }
        }

        /// <summary>
        ///     Returns the name of the current scenario.
        /// </summary>
        /// <returns>Current scenario name</returns>
        public string GetCurrentScenarioName()
        {
            return currentScenarioName;
        }

        /// <summary>
        ///     Returns a list of all available scenario names.
        /// </summary>
        /// <returns>List of scenario names</returns>
        public List<string> GetAvailableScenarioNames()
        {
            return availableScenarios?.Select(s => s.ScenarioName).ToList() ?? new List<string>();
        }

        #endregion
    }
} 