using System;
using System.Collections.Generic;
using UnityEngine;

namespace VARLab.DLX
{
    /// <summary>
    ///     This class is used to create a scenario data object. This object will contain the scenario name and a list of inspectables and their states.
    /// </summary>
    [CreateAssetMenu(fileName = "ScenarioData", menuName = "ScriptableObjects/ScenarioData")]
    public class ScenarioData : ScriptableObject
    {
        public string ScenarioName;

        [Serializable]
        public struct Inspectable
        {
            public string InspectableID;
            public string InspectableState;

            public Inspectable(string inspectableName, string inspectableState)
            {
                InspectableID = inspectableName;
                InspectableState = inspectableState;
            }
        }

        public List<Inspectable> InspectablesAndStates;
    }
}
