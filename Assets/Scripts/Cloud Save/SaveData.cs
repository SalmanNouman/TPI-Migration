using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using VARLab.CloudSave;

namespace VARLab.DLX
{
    /// <summary>
    /// This class is the SaveData object which will store the information that we wish to save, SaveDataSupport is the class that should be 
    /// interacting with this class to move data between the TPI scene and into this object. Upon save triggers it will be serialized 
    /// then sent to the API.
    /// </summary>
    [CloudSaved]
    [JsonObject(MemberSerialization.OptIn)]
    public class SaveData : MonoBehaviour
    {
        [JsonProperty]
        public Dictionary<string, string> PhotoIdAndTimeStamp = new();

        [JsonProperty]
        public List<ActivityData> ActivityLog;

        [JsonProperty]
        public List<InspectionSaveData> InspectionLog;

        [JsonProperty]
        public string LastPOI;

        [JsonProperty]
        public List<string> VisitedPOIs = new();

        [JsonProperty]
        public string Version;

        [JsonProperty]
        public bool EndInspection = false;

        [JsonProperty]
        public bool PiercerInteractionCompleted = false;

        [JsonProperty]
        public bool TattooArtistInteractionCompleted = false;

        [JsonProperty]
        public TimeSpan Time;

        [JsonObject(MemberSerialization.OptIn)]
        public struct ActivityData
        {
            [JsonProperty]
            public bool IsPrimary;

            [JsonProperty]
            public string LogString;
        }

        [JsonObject(MemberSerialization.OptIn)]
        public struct InspectionSaveData
        {
            [JsonProperty]
            public string ObjectId;

            [JsonProperty]
            public bool IsCompliant;

            [JsonProperty]
            public bool HasPhoto;
        }
    }
}
