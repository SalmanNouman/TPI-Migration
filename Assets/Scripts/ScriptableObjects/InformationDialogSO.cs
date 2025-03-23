using System;
using UnityEngine;

namespace VARLab.DLX
{
    [CreateAssetMenu(fileName = "InformationDialogSO", menuName = "ScriptableObjects/InformationDialogSO")]
    public class InformDialog : ScriptableObject
    {
        [Header("Content"), Space(5)]
        [TextArea(1, 3), Tooltip("The [Name] label of the information dialog")]
        public string Name;

        [TextArea(1, 3), Tooltip("The [Title] label of the information dialog. If empty, DisplayStyle.None will be set")]
        public string Title;

        [TextArea(1, 10), Tooltip("The [Description] label of the information dialog")]
        public string Description;

        [TextArea(1, 3), Tooltip("The text of the primary button.")]
        public string PrimaryBtnText;

        [Header("Additional Options"), Space(5)]
        [Tooltip("Whether the [Description] label is bolded or not")]
        public bool IsDescriptionBolded = true;

        [Tooltip("Whether the background behind the UI is dimmed or not")]
        public bool IsBackgroundDimmed = true;

        [Tooltip("Whether the Icon is visible or not")]
        public bool IsIconVisible = false;

        // Action delegate for button click
        private Action primaryAction;

        /// <summary>
        /// Sets the action to be performed when the primary button is clicked
        /// </summary>
        /// <param name="action">The action to perform</param>

        public void SetPrimaryAction(Action action)
        {
            primaryAction = action;
        }

        /// <summary>
        /// Invokes the primary action if it has been set
        /// </summary>
        public void InvokePrimaryAction()
        {
            primaryAction?.Invoke();
        }
    }
}