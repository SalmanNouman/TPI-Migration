using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using VARLab.DLX;

namespace VARLab.PublicHealth
{
    public class AudioManager : MonoBehaviour
    {
        [FormerlySerializedAs("_audioMixer")] 
        [SerializeField, Tooltip("Reference to the Audio Mixer")] private AudioMixer audioMixer;

        private Poi currentPOI;
        private List<AudioSource> audioSourcesCurrentPOI;

        private const float MinVolume = -80f;
        private const float MaxVolume = 0f;
        private float currentVolume = MaxVolume;

        private void Awake()
        {
            audioSourcesCurrentPOI = new();
        }
        /// <summary>
        /// <see cref="POIManager.POIChanged"/>
        /// a simple function to go through all audio sources under a POI and turn them on or off a bit too powerful might need a more fine tuned version later
        /// </summary>
        /// <param name="currentPOI"> POI to look through for audio sources</param>
        /// <param name="enable">turn on or off the sources</param>
        public void TogglePoiSFX(Poi enteredPoi)
        {
            if (currentPOI != enteredPoi)
            {
                foreach (var audioSource in audioSourcesCurrentPOI)
                {
                    ToggleAudioClips(audioSource, false);
                }

                audioSourcesCurrentPOI.Clear();

                audioSourcesCurrentPOI = enteredPoi.gameObject.GetComponentsInChildren<AudioSource>().ToList();

                foreach (var audioSource in audioSourcesCurrentPOI)
                {
                    ToggleAudioClips(audioSource, true);
                }

                currentPOI = enteredPoi;
            }
        }

        private void ToggleAudioClips(AudioSource audioSource, bool enable)
        {
            audioSource.enabled = enable;
        }

        public void SetVolume(string group, float value)
        {
            audioMixer.SetFloat(group, value);
        }

        
        /// <summary>
        /// Mutes/Unmutes the master volume
        /// </summary>
        /// <param name="group">name of the audio group that will be muted/unmuted. 
        /// Currently this function is only available for the master volume</param>
        /// <param name="enabled">
        /// True = sets the Master volume to the previously set volume
        /// False = saves the current volume and sets the Master Volume to -80</param>
        public void ToggleMasterVolume(bool enabled)
        {
            if (!enabled)
            {
                audioMixer.GetFloat("Volume", out currentVolume);
                audioMixer.SetFloat("Volume", MinVolume);
            }
            else
            {
                audioMixer.SetFloat("Volume", currentVolume);
            }
        }

        public float GetVolume(string group)
        {
            audioMixer.GetFloat(group, out float volume);
            return volume;
        }
    }
}
