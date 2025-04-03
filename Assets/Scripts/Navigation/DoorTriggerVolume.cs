using System.Collections;
using UnityEngine;

namespace VARLab.DLX
{
    public class DoorTriggerVolume : MonoBehaviour
    {
        [SerializeField, Tooltip("Reference to the door that will be animated")]
        private Transform door;

        [SerializeField, Tooltip("Value of End Rotation")]
        private float endRotation;

        [SerializeField, Tooltip("Value of Start Rotation")]
        private float startRotation;

        public float speed = 5;

        private Quaternion targetOpenRotation;
        private Quaternion targetCloseRotation;
        bool isOpen;

        private void Start()
        {
            targetOpenRotation = Quaternion.Euler(door.rotation.y, endRotation, door.rotation.z);
            targetCloseRotation = Quaternion.Euler(door.rotation.y, startRotation, door.rotation.z);
        }

        private void Update()
        {
            Quaternion currentTarget = isOpen ? targetOpenRotation : targetCloseRotation;

            door.rotation = Quaternion.Slerp(door.rotation, currentTarget, Time.deltaTime * speed);
        }
        private void OnTriggerEnter(Collider other)
        {
            isOpen = true;
            Debug.Log("Door trigger enter");
        }

        private void OnTriggerExit(Collider other)
        {
            isOpen  = false;
            Debug.Log("Door trigger exit");
        }
    }
}
