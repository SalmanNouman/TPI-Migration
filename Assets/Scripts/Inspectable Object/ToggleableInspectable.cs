using System.Collections.Generic;
using UnityEngine;

namespace VARLab.DLX
{

    public class ToggleableInspectable : InspectableObject
    {
        /// <summary>
        /// This method toggles the objects in the toggleables list setting their active state to the opposite value
        /// </summary>
        [Header("Toggleable Object Properties"), Space(5f)]
        [Tooltip("This is a list of game objects to turn on/off when inspected")] public List<GameObject> Toggleables = new();
        public override void InspectionStarted()
        {
            base.InspectionStarted();
            foreach (GameObject toggleable in Toggleables)
            {
                toggleable.SetActive(!toggleable.activeSelf);
            }
        }
        /// <summary>
        /// Invoked when the inspection completes.
        /// Toggles the active state of objects in the toggleables list.
        /// </summary>
        public override void InspectionCompleted()
        {
            base.InspectionCompleted();
            foreach (GameObject toggleable in Toggleables)
            {
                toggleable.SetActive(!toggleable.activeSelf);
            }
        }
    }
}
