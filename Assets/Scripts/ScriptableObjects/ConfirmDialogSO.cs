using System;
using UnityEngine;
using VARLab.Velcro;

namespace VARLab.DLX
{
    [CreateAssetMenu(fileName = "ConfirmDialogSO", menuName = "ScriptableObjects/Dialogs/ConfirmDialogSO")]
    public class ConfirmDialogSO : ScriptableObject
    {
        [Header("Content"), Space(5)]
        [TextArea(1, 3), Tooltip("The [Name] label of the confirmation dialog")]
        public string Name;

        [TextArea(1, 10), Tooltip("The [Description] label of the confirmation dialog")]
        public string Description;

        [Header("Button Options"), Space(5)]
        [Tooltip("The type of button the primary button will be (Right most button)")]
        public ButtonType PrimaryBtnType = ButtonType.Primary;

        [TextArea(1, 3), Tooltip("The text of the primary button.")]
        public string PrimaryBtnText;

        [Tooltip("The type of button the secondary button will be (Left most button)")]
        public ButtonType SecondaryBtnType = ButtonType.Secondary1;

        [TextArea(1, 3), Tooltip("The text of the secondary button")]
        public string SecondaryBtnText;

        [Header("Additional Options"), Space(5)]
        [Tooltip("Whether the \"X\" button in the top right is visible or not")]
        public bool IsCloseBtnVisible = true;

        [Tooltip("Whether the background behind the UI is dimmed or not")]
        public bool IsBackgroundDimmed = true;

        [Tooltip("Whether the dialog has a checkbox or not")]
        public bool IsCheckboxVisible = false;

        [TextArea(1, 3), Tooltip("The text of the checkbox")]
        public string CheckBoxText;

        // Window width and height settings
        [Header("Window Settings in Pixels"), Space(5)]

        [Tooltip("The width of the confirmation dialog window")]
        public int WindowWidth;

        [Tooltip("The height of the confirmation dialog window")]
        public int WindowHeight;

        // Action delegates for button clicks and checkbox toggle
        private Action primaryAction;
        private Action secondaryAction;
        private Action<bool> toggleAction;

        /// <summary>
        /// Sets the action to be performed when the primary button is clicked
        /// </summary>
        /// <param name="action">The action to perform</param>
        public void SetPrimaryAction(Action action)
        {
            primaryAction = action;
        }

        /// <summary>
        /// Sets the action to be performed when the secondary button is clicked
        /// </summary>
        /// <param name="action">The action to perform</param>
        public void SetSecondaryAction(Action action)
        {
            secondaryAction = action;
        }

        /// <summary>
        /// Sets the action to be performed when the checkbox is toggled
        /// </summary>
        /// <param name="action">The action to perform with the toggle state</param>
        public void SetToggleAction(Action<bool> action)
        {
            toggleAction = action;
        }

        /// <summary>
        /// Invokes the primary action if it has been set
        /// </summary>
        public void InvokePrimaryAction()
        {
            primaryAction?.Invoke();
        }

        /// <summary>
        /// Invokes the secondary action if it has been set
        /// </summary>
        public void InvokeSecondaryAction()
        {
            secondaryAction?.Invoke();
        }

        /// <summary>
        /// Invokes the toggle action with the provided state if it has been set
        /// </summary>
        /// <param name="isChecked">The state of the checkbox</param>
        public void InvokeToggleAction(bool isChecked)
        {
            toggleAction?.Invoke(isChecked);
        }
    }
}