using UnityEngine;
using UnityEngine.UIElements;
using VARLab.Velcro;

/// <summary>
/// This script handles updating the UI elements in the settings menu when settings are loaded from a save file.
/// It connects to the CustomSaveHandler events and updates the UI elements accordingly.
/// </summary>
public class LoadSettings : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private SlideToggle soundToggle;
    private Slider masterVolumeSlider;
    private Slider soundEffectsSlider;
    private Slider dialogueSlider;
    private Slider cameraSensitivitySlider;
    
    // Flag to prevent initial slider callbacks from saving values
    private bool isLoadingSettings = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the UIDocument component
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("LoadSettings: UIDocument component not found on this GameObject!");
            return;
        }
        
        // Get UI references and register callbacks
        GetUIReferences();
        RegisterCallbacks();
    }
    
    /// <summary>
    /// Gets references to all UI elements needed for settings
    /// </summary>
    private void GetUIReferences()
    {
        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("LoadSettings: Root visual element is null!");
            return;
        }
        
        // Get UI elements
        soundToggle = root.Q<TemplateContainer>("SoundToggle")?.Q<SlideToggle>();
        masterVolumeSlider = root.Q<TemplateContainer>("VolumeSlider")?.Q<FillSlider>();
        soundEffectsSlider = root.Q<TemplateContainer>("SoundEffectSlider")?.Q<FillSlider>();
        dialogueSlider = root.Q<TemplateContainer>("DialogueSlider")?.Q<FillSlider>();
        cameraSensitivitySlider = root.Q<TemplateContainer>("SensitivitySlider")?.Q<FillSlider>();
        
        // Log any missing UI elements
        if (soundToggle == null) Debug.LogWarning("LoadSettings: Sound toggle not found!");
        if (masterVolumeSlider == null) Debug.LogWarning("LoadSettings: Master volume slider not found!");
        if (soundEffectsSlider == null) Debug.LogWarning("LoadSettings: Sound effects slider not found!");
        if (dialogueSlider == null) Debug.LogWarning("LoadSettings: Dialogue slider not found!");
        if (cameraSensitivitySlider == null) Debug.LogWarning("LoadSettings: Camera sensitivity slider not found!");
    }
    
    /// <summary>
    /// Registers callbacks for UI elements
    /// </summary>
    private void RegisterCallbacks()
    {
        // Register for the RegisterValueChangedCallback event on each slider
        if (masterVolumeSlider != null)
            masterVolumeSlider.RegisterCallback<ChangeEvent<float>>(OnMasterVolumeSliderChanged);
        if (soundEffectsSlider != null)
            soundEffectsSlider.RegisterCallback<ChangeEvent<float>>(OnSoundEffectsSliderChanged);
        if (dialogueSlider != null)
            dialogueSlider.RegisterCallback<ChangeEvent<float>>(OnDialogueSliderChanged);
    }
    
    // Custom callbacks to prevent saving during loading
    private void OnMasterVolumeSliderChanged(ChangeEvent<float> evt)
    {
        // Only allow the original callback to save if we're not currently loading settings
        if (isLoadingSettings)
            evt.StopPropagation();
    }
    
    private void OnSoundEffectsSliderChanged(ChangeEvent<float> evt)
    {
        if (isLoadingSettings)
            evt.StopPropagation();
    }
    
    private void OnDialogueSliderChanged(ChangeEvent<float> evt)
    {
        if (isLoadingSettings)
            evt.StopPropagation();
    }
    
    /// <summary>
    /// Updates the sound toggle UI element when the sound toggle setting is loaded
    /// </summary>
    /// <param name="isEnabled">Whether sound is enabled</param>
    public void UpdateSoundToggle(bool isEnabled)
    {
        // Update the UI if available
        if (soundToggle != null)
            soundToggle.value = isEnabled;
    }

    /// <summary>
    /// Updates the master volume slider UI element when the master volume setting is loaded
    /// </summary>
    /// <param name="group">The audio mixer group name</param>
    /// <param name="value">The volume value (logarithmic)</param>
    public void UpdateMasterVolume(string group, float value)
    {
        // Update the UI if available
        if (masterVolumeSlider != null && group == "Volume")
        {
            isLoadingSettings = true;
            // Convert from dB (-80 to 0) to linear (0 to 1)
            masterVolumeSlider.value = SettingsMenuHelper.ConvertLogVolumeToLinear(value);
            isLoadingSettings = false;
            Debug.Log($"LoadSettings: Updated master volume slider to {masterVolumeSlider.value} (from dB value {value})");
        }
    }

    /// <summary>
    /// Updates the sound effects volume slider UI element when the sound effects volume setting is loaded
    /// </summary>
    /// <param name="group">The audio mixer group name</param>
    /// <param name="value">The volume value (logarithmic)</param>
    public void UpdateSoundEffectsVolume(string group, float value)
    {
        // Update the UI if available
        if (soundEffectsSlider != null && group == "SoundEffects")
        {
            isLoadingSettings = true;
            // Convert from dB (-80 to 0) to linear (0 to 1)
            soundEffectsSlider.value = SettingsMenuHelper.ConvertLogVolumeToLinear(value);
            isLoadingSettings = false;
            Debug.Log($"LoadSettings: Updated sound effects slider to {soundEffectsSlider.value} (from dB value {value})");
        }
    }

    /// <summary>
    /// Updates the dialogue volume slider UI element when the dialogue volume setting is loaded
    /// </summary>
    /// <param name="group">The audio mixer group name</param>
    /// <param name="value">The volume value (logarithmic)</param>
    public void UpdateDialogueVolume(string group, float value)
    {
        // Update the UI if available
        if (dialogueSlider != null && group == "Dialogue")
        {
            isLoadingSettings = true;
            // Convert from dB (-80 to 0) to linear (0 to 1)
            dialogueSlider.value = SettingsMenuHelper.ConvertLogVolumeToLinear(value);
            isLoadingSettings = false;
            Debug.Log($"LoadSettings: Updated dialogue slider to {dialogueSlider.value} (from dB value {value})");
        }
    }

    /// <summary>
    /// Updates the camera sensitivity slider UI element when the camera sensitivity setting is loaded
    /// </summary>
    /// <param name="value">The camera sensitivity value (0-1)</param>
    public void UpdateCameraSensitivity(float value)
    {
        // Update the UI if available
        if (cameraSensitivitySlider != null)
            cameraSensitivitySlider.value = value;
    }
}
