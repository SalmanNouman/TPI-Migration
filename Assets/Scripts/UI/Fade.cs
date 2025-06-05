using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace VARLab.DLX
{
    public class Fade : MonoBehaviour
    {
        /// <summary>
        ///    The singleton instance of the Fade class.
        /// </summary>
        public static Fade Instance;

        /// <summary>
        ///    The root visual element
        /// </summary>
        private VisualElement root;

        /// <summary>
        ///     The visual element that will be faded in and out that is set in start.
        /// </summary>
        private VisualElement fade;



        private const string FadeOutUSS = "fade-out";
        private const string FadeInUSS = "fade-in";
        private const float FadeTime = 1.5f;

        void Start()
        {
            Instance = this;
            root = GetComponent<UIDocument>().rootVisualElement;
            fade = root.Q<VisualElement>("FadeContainer");
            root.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// This is a method that allows for an action to be done behind the fade.
        /// </summary>
        /// <param name="act">The action to be executed. </param>
        /// <returns></returns>
        public IEnumerator FadeButtonCoroutine(Action act)
        {
            yield return FadeInCoroutine();

            act();

            yield return new WaitForSeconds(1f);

            yield return FadeOuCoroutine();
        }

        /// <summary>
        /// This method will add the fade in uss class to the visual element
        /// to gradually turn the screen black over a predefined timespan.
        /// </summary>
        private IEnumerator FadeInCoroutine()
        {
            yield return new WaitForFixedUpdate();

            fade.AddToClassList(FadeInUSS);
            root.style.display = DisplayStyle.Flex;
            

            yield return new WaitForSeconds(FadeTime);
        }

        /// <summary>
        /// This method will add the fade out uss class to the visual element
        /// to gradually dismiss the fade over a predefined timespan.
        /// Removes both fave in and fade out uss classes.
        /// </summary>
        private IEnumerator FadeOuCoroutine()
        {
            yield return new WaitForFixedUpdate();

            fade.AddToClassList(FadeOutUSS);
            yield return new WaitForSeconds(FadeTime);

            root.style.display = DisplayStyle.None;

            fade.RemoveFromClassList(FadeOutUSS);
            fade.RemoveFromClassList(FadeInUSS);
        }
    }
}
