using System.Collections.Generic;
using UnityEngine;

namespace VARLab.DLX
{

    public class ToggleableInspectable : InspectableObject
    {

        [Header("Toggleable Object Properties"), Space(5f)]
        [Tooltip("This is a list of game objects to turn on/off when inspected")] public List<GameObject> Toggleables = new();

        public void ToggleForInspection()
        {
            foreach (var toggleable in Toggleables)
            {
                toggleable.SetActive(!toggleable.activeSelf);
            }
        }
    }
}
