using EPOOutline;
using UnityEngine;

namespace VARLAB.DLX
{
    /// <summary>
    ///     Provides an interface between the EPOOutline package and CORE Interactions
    ///     by providing methods which can listen to mouse events and change the state of
    ///     an object with an <see cref="Outlinable"/> component
    /// </summary>
    /// <remarks>
    ///     This code was originally developed in the CORE Sandbox and has been imported
    ///     into the DLX Project Template
    /// </remarks>
    public class OutlineInteraction : MonoBehaviour
    {

        // Global colour preferences could be applied here


        [Tooltip("A layer that is included in the Outliner layer mask")]
        public int VisibleLayer = 1;

        [Tooltip("A layer not included in the Outliner layer mask. " +
            "The Outline will be hidden when set to this layer.")]
        public int HiddenLayer = 0;


        /// <summary>
        ///     If the received <paramref name="obj"/> has an <see cref="Outlinable"/>
        ///     component, the layer of the Outline is changed so that the layer is visible
        /// </summary>
        /// <param name="obj">A relevant GameObject</param>
        public void ShowOutline(GameObject obj)
        {
            if (!obj) { return; }

            Outlinable outline = obj.GetComponent<Outlinable>();



            if (!outline) { return; }

            outline.OutlineLayer = VisibleLayer;
        }

        /// <summary>
        ///     If the received <paramref name="obj"/> has an <see cref="Outlinable"/>
        ///     component, the layer of the Outline is changed so that the layer is hidden.
        /// </summary>
        /// <param name="obj">A relevant GameObject</param>
        public void HideOutline(GameObject obj)
        {
            if (!obj) { return; }

            Outlinable outline = obj.GetComponent<Outlinable>();

            if (!outline) { return; }

            outline.OutlineLayer = HiddenLayer;
        }
    }
}
