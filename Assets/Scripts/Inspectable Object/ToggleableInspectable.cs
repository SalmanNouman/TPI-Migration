using System.Collections.Generic;
using UnityEngine;

namespace VARLab.DLX
{

    public class ToggleableInspectable : InspectableObject
    {

        [Header("Toggleable Object Properties"), Space(5f)]
        [Tooltip("This is a list of game objects to turn on/off when inspected")] public List<GameObject> Toggleables = new();

        /// <summary>
        /// Subscribes the ToggleObjects method to the OnObjectClicked and OnObjectInspected events.
        /// </summary>
        private void Start()
        {
            OnObjectClicked.AddListener(ToggleObjects);
            OnObjectInspected.AddListener(ToggleObjects);
        }

        /// <summary>
        /// This method toggles the objects in the toggleables list setting their active state to the opposite value
        /// </summary>
        private void ToggleObjects(InspectableObject inspectable)
        {
            foreach (var obj in Toggleables)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
    }
}
