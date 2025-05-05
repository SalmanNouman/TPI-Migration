using UnityEngine;
using UnityEngine.Events;

namespace VARLab.DLX
{
    public abstract class Tasks : MonoBehaviour
    {
        //events
        public UnityEvent OnTaskStarted;
        public UnityEvent OnTaskCompleted;
        public UnityEvent OnTaskFailed;

        //methods
        public abstract void HandleTask();
    }
}
