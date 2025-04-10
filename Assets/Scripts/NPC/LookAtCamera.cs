using UnityEngine;

namespace VARLab.DLX
{
    /// <summary>
    ///     This class is used on NPC to make the NPC look at the camera, but only rotate around the Y-axis.
    /// </summary>
    public class LookAtCamera : MonoBehaviour
    {
        public Transform Rotator; //where the object will rotate from. Can be assigned to an empty parent to get more desirable results

        public float LookAtDistance = 7; //the distance at which the rotator will attempt to face the target. When the target is outside of this radial check, the rotator returns to its default orientation
        public float LookAtSpeed = 3; //the speed at which the rotator will turn

        public Transform LookAtTarget; //where the object should look towards. Generally, this will be the player object, or the player's camera

        private Quaternion startRotation; //saves the beginning orientation of the object to be returned to if the player leaves the range
        private Quaternion currentRotation; //store the direction the rotator is currently facing

        void Start()
        {
            startRotation = Rotator.rotation; //store the initial orientation at start
            currentRotation = startRotation; //initialize current rotation
        }

        void LateUpdate() //late update is used so that this script can override animation information
        {
            if (LookAtDistance > Vector3.Distance(Rotator.position, LookAtTarget.position))
            {
                // Get direction to target but zero out the y component to get only horizontal direction
                Vector3 directionToTarget = LookAtTarget.position - Rotator.position;
                directionToTarget.y = 0; // Ignore vertical difference
                
                // Only if we have a valid direction (not zero magnitude)
                if (directionToTarget.sqrMagnitude > 0.001f)
                {
                    // Create rotation that only rotates around Y axis
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    
                    // Preserve original rotation except for Y axis rotation
                    targetRotation = Quaternion.Euler(startRotation.eulerAngles.x, targetRotation.eulerAngles.y, startRotation.eulerAngles.z);
                    
                    // Smoothly interpolate to the target rotation
                    currentRotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * LookAtSpeed);
                    Rotator.rotation = currentRotation;
                }
            }
            else
            {
                // Return to starting rotation when player leaves range
                currentRotation = Quaternion.Slerp(currentRotation, startRotation, Time.deltaTime * LookAtSpeed);
                Rotator.rotation = currentRotation;
            }
        }
    }
}
