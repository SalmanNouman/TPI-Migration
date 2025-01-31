using System;
using UnityEngine;


namespace VARLab.DLX
{
    [Serializable]
    public class State
    {
        public GameObject InspectableGameObject;
        public Compliancy Compliancy;
        /// <summary>
        /// Initializes a new instance of the State class.
        /// </summary>
        public State(GameObject obj, Compliancy comp)
        {
            InspectableGameObject = obj;
            Compliancy = comp;
        }
    }
}
