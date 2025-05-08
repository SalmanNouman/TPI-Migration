using System;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VARLab.PublicHealth;

namespace Tests.PlayMode
{
    public class AudioManagerTests
    {
        private AudioManager audioManager;

        // private List<AudioSource> _audioSources;
        private GameObject mockGameObject;

        private const string MasterVolume = "Volume";


        [UnitySetUp]
        public IEnumerator OneTimeSetup()
        {
            mockGameObject = new GameObject("AudioManagerMock");
            audioManager = mockGameObject.AddComponent<AudioManager>();
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/Audio/TPIAudioMixer.mixer");

            Assert.IsNotNull(mixer, "Mixer could not be loaded. Make sure it's in a Resources folder.");

            // Set private 'mixer' field using reflection
            var mixerField =
                typeof(AudioManager).GetField("audioMixer", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(mixerField, "Could not find 'audioMixer' field on AudioManager.");
            mixerField.SetValue(audioManager, mixer);

            yield return null;
        }


        // A Test behaves as an ordinary method
        [Test]
        public void CheckMasterVolumeIsSetToZero()
        {
            var expectedVolume = 0f;

            Assert.AreEqual(expectedVolume, audioManager.GetVolume(MasterVolume));
        }

        [Test]
        public void SetVolumeToMinusTen()
        {
            var expectedVolume = -10f;

            audioManager.SetVolume(MasterVolume, -10f);

            Assert.AreEqual(expectedVolume, Math.Round(audioManager.GetVolume(MasterVolume), 1));
        }

        [UnityTest]
        public IEnumerator MuteMasterVolume()
        {
            var expectedVolume = -80f;

            audioManager.ToggleMasterVolume(false);

            yield return null;

            Assert.AreEqual(expectedVolume, audioManager.GetVolume(MasterVolume));
        }

        [UnityTest]
        public IEnumerator UnmuteMasterVolume()
        {
            var expectedVolume = -0.5f;

            audioManager.SetVolume(MasterVolume, -0.5f);

            audioManager.ToggleMasterVolume(false);

            yield return new ();

            audioManager.ToggleMasterVolume(true);

            yield return null;

            Assert.AreEqual(expectedVolume, Math.Round(audioManager.GetVolume(MasterVolume), 2));
        }
    }
}
