using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class ThemeManager : MonoBehaviour
{
    [Tooltip("Panel Settings for TPI UI")]
    public PanelSettings TpiPanelSettings;
    [Header("Themes")]
    [SerializeField]
    public ThemeStyleSheet DefaultTheme;
    [SerializeField]
    public ThemeStyleSheet DarkModeTheme;
    [SerializeField]
    public ThemeStyleSheet LightModeTheme;


    public void Awake()
    { 
        TpiPanelSettings.themeStyleSheet = DefaultTheme;
    }

    /// <summary>
    /// Checks which theme is currently active and sets the opposite of that.
    /// <see cref="SettingsMenuComplex.OnThemeTogglePressed"/>
    /// </summary>
    public void ToggleTheme()
    {
        if (TpiPanelSettings.themeStyleSheet == DarkModeTheme)
        {
            TpiPanelSettings.themeStyleSheet = LightModeTheme;
        }
        else
        {
            TpiPanelSettings.themeStyleSheet = DarkModeTheme;
        }
    }
}
