using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace VARLab.DLX
{
    /// <summary>
    /// Original singleton manager from LSM7.07 Motive Power Circuits: 
    /// https://bitbucket.org/VARLab/motive-power-circuits/src/development/Assets/Scripts/Game%20Managers/VoiceOverManager.cs
    /// Minor changes to the class to fit the project
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [DisallowMultipleComponent]
    public class VoiceOverManager : MonoBehaviour
    {
        public static VoiceOverManager Instance { get; private set; }
        private AudioSource audioSource;
        private Coroutine playAudioClips = null;

        public bool IsPlaying { get { return playAudioClips != null || audioSource.isPlaying; } }

        public bool IsPaused;

        private void Awake()
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
        }
        
        public void PlayAudioClipsFromList(List<AudioClip> audioClips, float startDelay = 0f, float timeBetweenClips = 0f)
        {
            // if the list is empty return
            if (audioClips == null)
            {
                return;
            }

            //if something is playing in the audio source stop.
            audioSource.Stop();

            //if the audioclip coroutine is not null stop it.
            if (playAudioClips != null)
            {
                StopCoroutine(playAudioClips);
            }

            //play all clips
            playAudioClips = StartCoroutine(PlayAudioClipsCoroutine(audioClips, startDelay, timeBetweenClips));
        }
        

        private IEnumerator PlayAudioClipsCoroutine(List<AudioClip> audioclips, float startDelay = 0f, float timeBetweenInstructions = 0f)
        {
            yield return new WaitForSeconds(startDelay);
            yield return new WaitUntil(() => !IsPaused);

            // Play the current audio segment
            foreach (var audioClip in audioclips)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(audioClip);
                yield return new WaitForSeconds(audioClip.length + timeBetweenInstructions);
                yield return new WaitUntil(() => !IsPaused);
                yield return new WaitUntil(() => !audioSource.isPlaying);
            }

            playAudioClips = null;
        }

        /// <summary>
        /// Stop the audio clip
        /// </summary>
        public void StopAudioClip()
        {
            audioSource.Stop();
        }

        /// <summary>
        /// Stop all audio clips
        /// </summary>
        public void StopAllClips()
        {
            if (playAudioClips != null)
                StopCoroutine(playAudioClips);

            audioSource.Stop();
        }
    }
}