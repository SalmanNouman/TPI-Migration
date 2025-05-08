using UnityEngine;
using VARLab.Navigation.PointClick;
using VARLab.ObjectViewer;

/// <summary>
/// Since Velcro sensitivity uses a value range from 0 to 1, and the pan sensitivity in PointClickNavigation 
/// cannot be modified through events or directly changed (as it's part of an external package), adjustments 
/// must be handled through a wrapper class.
/// </summary>
public class CameraSensitivityChanger : MonoBehaviour
{
    [SerializeField]
    public PointClickNavigation PointClickNavigation;
    [SerializeField]
    public ObjectViewerController ObjectViewerController;
    
    private const float PointClickNavSenseModifier= 4.0f;
    private const float ObjectViewerMouseControllerSenseModifier = 1000f;
    private const float ObjectViewerKeyboardControllerSenseModifier = 250f;
    
    /// <summary>
    /// Changes the camera sensitivity by value * the point and clicks navigations max sensitivity range
    /// </summary>
    /// <see cref="ComplexSettingsMenu.OnCameraSlider"/>
    /// <param name="value"></param>
    public void SetCameraSensitivity(float value)
    {
        float senseValue = value;
        if (value <= 0)
        {
            senseValue = 0.1f;
        }
        PointClickNavigation.CameraPanSensitivity = senseValue * PointClickNavSenseModifier;
        ObjectViewerController.MouseSensitivity = senseValue * ObjectViewerMouseControllerSenseModifier;
        ObjectViewerController.KeyboardSensitivity = senseValue * ObjectViewerKeyboardControllerSenseModifier;
    }
}
