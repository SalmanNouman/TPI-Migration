using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VARLab.DLX;
using Button = UnityEngine.UIElements.Button;

/// <summary>
///     Editor window for managing inspectable object states. This window will allow
///     loading and saving of inspectable object states for different POIs using the
///     Scenario files created in the editor.
/// </summary>
public class InspectableStateEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset rootInspectableStateEditor = default;

    [SerializeField] private VisualTreeAsset inspectableFoldout = default;

    [SerializeField] private VisualTreeAsset inspectableDropdown = default;

    private Dictionary<string, List<InspectableObject>> poiAndInspectables;

    private ScrollView scrollView;

    private TextField fileName;


    /// <summary>
    ///     Constant string for the directory that the scenario's will be saved to
    /// </summary>
    private const string ScenariosDirectoryPath = "Assets/Resources/Scenarios/";
    private const string MenuPath = "TPI/Inspectable State Editor";
    private const string WindowTitle = "Inspectable State Editor";
    private const string LoadFileDropdownField = "ddf_loadFile";
    private const string StatesScrollView = "sv_states";
    private const string FileNameTextField = "tf_fileName";
    private const string SaveScenarioBtn = "btn_saveScenario";
    private const string LoadScenarioBtn = "btn_ApplyScenario";
    private const string SaveTextBtn = "Save New Scenario";
    private const string SaveUpdateTextBtn = "Update Scenario File";
    private const string ApplyTextBtn = "Apply To Scene";

    /// <summary>
    ///     This builds the window and sets the min size that the window can be. 
    /// </summary>
    [MenuItem(MenuPath)]
    public static void ShowExample()
    {
        InspectableStateEditor wnd = GetWindow<InspectableStateEditor>();
        wnd.titleContent = new GUIContent(WindowTitle);
        //min size
        wnd.minSize = new Vector2(500, 800);
    }

    /// <summary>
    ///     This method setups up the UI elements and sets up the data that needs to be tracked from the user. 
    /// </summary>
    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = rootInspectableStateEditor.Instantiate();
        root.Add(labelFromUXML);
        root.RemoveFromClassList("StateViewItems");

        GetPOIsAndInspectableObjects();

        SetupData(root);
    }

    /// <summary>
    /// This function will generate and sort the dictionary _poiAndInspectables.
    /// </summary>
    public void GetPOIsAndInspectableObjects()
    {
        //Create a new dictionary
        poiAndInspectables = new();

        // Get all inspectable objects in the scene
        List<InspectableObject> inspectableObjects = Object.FindObjectsByType<InspectableObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();

        List<string> poiNames = new();

        // Create a list of unique POIs
        foreach (InspectableObject inspectableObject in inspectableObjects)
        {
            string location = inspectableObject.Location.ToString();

            if (!poiNames.Contains(location))
            {
                poiNames.Add(location);
            }
        }

        // Alphabetize the POI names and add to dictionary
        poiNames.Sort();

        foreach (string poi in poiNames)
        {
            poiAndInspectables.TryAdd(poi, new List<InspectableObject>());
        }

        // Get the inspectables in each POI
        foreach (InspectableObject inspectableObject in inspectableObjects)
        {
            string location = inspectableObject.Location.ToString();
            poiAndInspectables[location].Add(inspectableObject);
        }

        // Alphabetize the lists of inspectables
        foreach (string poi in poiAndInspectables.Keys)
        {
            poiAndInspectables[poi].Sort((p, q) => p.Name.CompareTo(q.Name));
        }
    }

    /// <summary>
    /// This method sets up the functionality within the UI and populates the data. 
    /// </summary>
    /// <param name="root">rootVisualElement</param>
    private void SetupData(VisualElement root)
    {
        //Get all the scenario files in the directory add them to the dropdown
        var scenarioFiles = Directory.GetFiles(ScenariosDirectoryPath, "*.asset").Select(Path.GetFileName).ToArray();

        //find the load file dropdown 
        var loadFileDropdown = root.Q<DropdownField>(LoadFileDropdownField);

        //drop the file extensions
        loadFileDropdown.choices = scenarioFiles.Select(file => Path.GetFileNameWithoutExtension(file)).ToList();
        loadFileDropdown.RegisterValueChangedCallback<string>(evt =>
        {
            //protect error against resetting the index of the dropdown
            if (loadFileDropdown.index < 0)
                return;

            UpdateScrollView(evt.newValue);
            fileName.value = evt.newValue;
        });

        //Body Section
        //find the scrollview 
        scrollView = root.Q<ScrollView>(StatesScrollView);
        PopulateScrollView(scrollView);

        //Footer Section
        //find the file name textfield
        fileName = root.Q<TextField>(FileNameTextField);
        fileName.RegisterValueChangedCallback(evt =>
        {
            //Change the text of the save button if the file name already exists
            if (loadFileDropdown.choices.Contains(evt.newValue))
            {
                root.Q<Button>(SaveScenarioBtn).text = SaveUpdateTextBtn;
            }
            else
            {
                root.Q<Button>(SaveScenarioBtn).text = SaveTextBtn;
            }

            if (AcceptableFileName(fileName.text))
            {
                //Set the button to be interactable
                root.Q<Button>(SaveScenarioBtn).SetEnabled(true);
            }
            else
            {
                //Set the button to be not interactable
                root.Q<Button>(SaveScenarioBtn).SetEnabled(false);
            }
        });

        //find the save button
        var saveButton = root.Q<Button>(SaveScenarioBtn);
        saveButton.text = SaveTextBtn;
        saveButton.SetEnabled(false);
        saveButton.clickable.clicked += () =>
        {
            CreateCurrentStateFile(scrollView, fileName.value);
            //Update the dropdown with the new file
            loadFileDropdown.choices = Directory.GetFiles(ScenariosDirectoryPath, "*.asset").Select(file => Path.GetFileNameWithoutExtension(file)).ToList();
            //Set the dropdown to the new file
            loadFileDropdown.value = fileName.value;
            //Change the text of the save button back to the original text
            saveButton.text = SaveUpdateTextBtn;
        };

        //find the load button
        var applyChangesButton = root.Q<Button>(LoadScenarioBtn);
        applyChangesButton.text = ApplyTextBtn;
        applyChangesButton.clickable.clicked += () =>
        {
            //Apply the changes made in the dropdowns to the inspectable objects in the scene
            ApplyChanges();
            //Apply the changes to the internal dictionary.
            GetPOIsAndInspectableObjects();

            loadFileDropdown.index = -1;
        };
    }

    /// <summary>
    /// Using the _poiAndInspectables dictionary made when the window was opened, 
    /// populate the foldouts and dropdowns with the poi's and the inspectable objects with states. 
    /// Show all inspectable objects, including those with only one state.
    /// </summary>
    /// <param name="scrollView"></param>
    private void PopulateScrollView(ScrollView scrollView)
    {
        if (poiAndInspectables == null)
        {
            Debug.LogError("No POIs and inspectables found. Make sure there are inspectable objects in the scene.");
            return;
        }

        //Iterate through the dictionary and create the foldouts and dropdowns
        foreach (KeyValuePair<string, List<InspectableObject>> entry in poiAndInspectables)
        {
            //Add the foldout to the scrollview
            //Create a new foldout using the foldout visual tree asset
            Foldout foldout = inspectableFoldout.Instantiate().Q<Foldout>();
            foldout.text = entry.Key.ToString();

            //Add the dropdowns to the foldout
            for (int i = 0; i < entry.Value.Count; i++)
            {
                // Generate or ensure ObjectId is set for this inspectable
                if (string.IsNullOrEmpty(entry.Value[i].ObjectId))
                {
                    entry.Value[i].GeneratedId();
                }

                // Create the dropdown for all inspectable objects
                DropdownField dropdownInspectables = inspectableDropdown.Instantiate().Q<DropdownField>();

                dropdownInspectables.label = entry.Value[i].Name;
                dropdownInspectables.choices = GetStateNamesFromGameObjects(entry.Value[i]);
                dropdownInspectables.index = 0;
                dropdownInspectables.AddToClassList("StateViewItems");

                // For objects with only one state, disable the dropdown but still show it
                if (entry.Value[i].States.Count <= 1)
                {
                    dropdownInspectables.SetEnabled(false);
                }

                foldout.Add(dropdownInspectables);
            }

            // Add the foldout to the scrollview if there is at least 1 inspectable
            if (foldout.childCount > 0)
            {
                scrollView.Add(foldout);
            }
        }
    }

    /// <summary>
    ///     This method will take the currently selected elements within the scroll view and populate a scriptable object dictionary. 
    /// </summary>
    /// <param name="sv">The scrollview that contains all the data from built from the <see cref="GetPOIsAndInspectableObjects"/> and whatever changes the user made to it</param>
    /// <param name="fileName">The file name entered in the file name text field</param>
    public void CreateCurrentStateFile(ScrollView sv, string fileName)
    {
        //Create a new scenario data object and name the object the value of fileName
        ScenarioData scenarioData = CreateInstance<ScenarioData>();

        //Set the scenario name
        scenarioData.ScenarioName = fileName;

        //Create a new list of inspectable objects
        List<ScenarioData.Inspectable> inspectableList = new();

        // Create a dictionary to track which inspectables we've already processed
        Dictionary<string, bool> processedInspectables = new Dictionary<string, bool>();

        // First, process all the inspectables from the UI dropdowns
        foreach (Foldout foldout in sv.Children().Cast<Foldout>())
        {
            string poiName = foldout.text;
            foreach (DropdownField dropdown in foldout.Children().Cast<DropdownField>())
            {
                string objectName = dropdown.label;
                string inspectableID = poiName + "_" + objectName;

                // Find the actual inspectable object to get its proper ObjectId
                InspectableObject inspectableObj = null;
                if (poiAndInspectables.TryGetValue(poiName, out List<InspectableObject> inspectables))
                {
                    inspectableObj = inspectables.Find(obj => obj.Name == objectName);
                }

                // Use the actual ObjectId from the inspectable object if available
                string finalId = inspectableObj != null ? inspectableObj.ObjectId : inspectableID;

                // Mark this inspectable as processed
                processedInspectables[finalId] = true;

                ScenarioData.Inspectable inspectable = new()
                {
                    InspectableID = finalId,
                    InspectableState = dropdown.value
                };

                inspectableList.Add(inspectable);
            }
        }

        // Now ensure we have entries for any inspectables with only one state
        if (poiAndInspectables != null)
        {
            foreach (KeyValuePair<string, List<InspectableObject>> entry in poiAndInspectables)
            {
                for (int i = 0; i < entry.Value.Count; i++)
                {
                    // Make sure ObjectId is set
                    if (string.IsNullOrEmpty(entry.Value[i].ObjectId))
                    {
                        entry.Value[i].GeneratedId();
                    }

                    // If we haven't processed this inspectable yet, add it with its default state
                    if (!processedInspectables.ContainsKey(entry.Value[i].ObjectId))
                    {
                        var states = entry.Value[i].GetListOfObjectStates();
                        if (states != null && states.Count > 0)
                        {
                            ScenarioData.Inspectable inspectable = new()
                            {
                                InspectableID = entry.Value[i].ObjectId,
                                InspectableState = states[0]
                            };

                            inspectableList.Add(inspectable);
                        }
                    }
                }
            }
        }

        //if the file exists, update the file otherwise create a new file
        if (File.Exists(ScenariosDirectoryPath + fileName + ".asset"))
        {
            //Load the file
            ScenarioData existingScenarioData = AssetDatabase.LoadAssetAtPath<ScenarioData>(ScenariosDirectoryPath + fileName + ".asset");

            //Update the list
            existingScenarioData.InspectablesAndStates = inspectableList;
#if UNITY_EDITOR
            EditorUtility.SetDirty(existingScenarioData);
#endif
        }
        else
        {
            //Add the list to the scenario data object
            scenarioData.InspectablesAndStates = inspectableList;

            //Create the file path
            string filePath = ScenariosDirectoryPath + fileName + ".asset";

            //Create the file
            AssetDatabase.CreateAsset(scenarioData, filePath);

            //Save assets
            AssetDatabase.SaveAssets();
        }

        //Save the changes
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    ///     This method will update the inspectable objects in the scene with the states provided within the scrollview
    /// </summary>
    private void ApplyChanges()
    {
        //Iterate through the dropdowns and apply the changes to the inspectable objects in the scene
        foreach (Foldout foldout in scrollView.Children().Cast<Foldout>())
        {
            string poiName = foldout.text;
            foreach (DropdownField dropdown in foldout.Children().Cast<DropdownField>())
            {
                string objectName = dropdown.label;

                // Find the inspectable object with the matching name and POI
                InspectableObject inspectableObject = null;
                if (poiAndInspectables.TryGetValue(poiName, out List<InspectableObject> inspectables))
                {
                    inspectableObject = inspectables.Find(obj => obj.Name == objectName);
                }

                if (inspectableObject != null)
                {
                    // Find the state that matches the selected value by GameObject name
                    foreach (var state in inspectableObject.States)
                    {
                        string stateName = state.InspectableGameObject != null ?
                            state.InspectableGameObject.name : state.Compliancy.ToString();

                        if (stateName == dropdown.value)
                        {
                            // Check if the target state has any children
                            bool hasChildren = false;
                            if (state.InspectableGameObject != null)
                            {
                                // Count active children in the target state
                                int childCount = state.InspectableGameObject.transform.childCount;
                                hasChildren = childCount > 0;
                            }

                            if (!hasChildren)
                            {
                                // If the target state has no children, deactivate the entire inspectable object
                                Debug.Log($"InspectableStateEditor: State '{stateName}' in '{inspectableObject.Name}' in '{inspectableObject.Location}' has no children. Deactivating the entire object.");
                                inspectableObject.gameObject.SetActive(false);
                            }
                            else
                            {
                                // Make sure the inspectable object is active
                                inspectableObject.gameObject.SetActive(true);
                                
                                // Activate the selected state's GameObject and deactivate others
                                foreach (var otherState in inspectableObject.States)
                                {
                                    if (otherState.InspectableGameObject != null)
                                    {
                                        otherState.InspectableGameObject.SetActive(otherState == state);
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    ///     This method will update the scrollview with the items from the file loaded from the load file dropdown.
    /// </summary>
    /// <param name="fileName">the name of the file to be loaded</param>
    private void UpdateScrollView(string fileName)
    {
        //Load the file
        ScenarioData scenarioData = AssetDatabase.LoadAssetAtPath<ScenarioData>(ScenariosDirectoryPath + fileName + ".asset");

        if (scenarioData == null)
        {
            Debug.LogError($"Failed to load scenario data at {ScenariosDirectoryPath + fileName + ".asset"}");
            return;
        }

        //Get the list of inspectable objects
        List<ScenarioData.Inspectable> inspectableList = scenarioData.InspectablesAndStates;

        //Iterate through the list and update the dropdowns
        foreach (Foldout foldout in scrollView.Children().Cast<Foldout>())
        {
            string poiName = foldout.text;
            foreach (DropdownField dropdown in foldout.Children().Cast<DropdownField>())
            {
                string objectName = dropdown.label;

                // First try to find the actual inspectable object to get its proper ObjectId
                string inspectableID = poiName + "_" + objectName;
                InspectableObject inspectableObj = null;

                if (poiAndInspectables.TryGetValue(poiName, out List<InspectableObject> inspectables))
                {
                    inspectableObj = inspectables.Find(obj => obj.Name == objectName);
                    if (inspectableObj != null && !string.IsNullOrEmpty(inspectableObj.ObjectId))
                    {
                        inspectableID = inspectableObj.ObjectId;
                    }
                }

                // Find the inspectable data with the matching ID
                var inspectableData = inspectableList.Find(inspectable => inspectable.InspectableID == inspectableID);

                if (inspectableData.InspectableState != null)
                {
                    dropdown.value = inspectableData.InspectableState;
                }
            }
        }
    }

    /// <summary>
    /// Gets a list of state names based on the GameObject names in the inspectable object's states
    /// </summary>
    /// <param name="inspectableObject">The inspectable object to get state names from</param>
    /// <returns>A list of state names for the dropdown</returns>
    private List<string> GetStateNamesFromGameObjects(InspectableObject inspectableObject)
    {
        List<string> stateNames = new List<string>();

        foreach (var state in inspectableObject.States)
        {
            if (state.InspectableGameObject != null)
            {
                // Use the GameObject name as the state identifier
                stateNames.Add(state.InspectableGameObject.name);
            }
            else
            {
                // Fallback to compliancy if no GameObject is assigned
                stateNames.Add(state.Compliancy.ToString());
            }
        }

        return stateNames;
    }

    /// <summary>
    ///     This method will check to ensure that the file name is acceptable for saving.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    private bool AcceptableFileName(string filename)
    {
        //Check to see if the file name is empty
        if (string.IsNullOrEmpty(filename))
        {
            return false;
        }

        //Check to see if the file name contains any special characters
        if (Regex.IsMatch(filename, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"))
        {
            return false;
        }

        return true;
    }
}
