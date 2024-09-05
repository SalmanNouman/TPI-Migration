using UnityEngine;
using VARLab.CloudSave;
using VARLab.DeveloperTools;

namespace VARLab.DLX
{

    /// <summary>
    ///     Extends the <see cref="ExperienceSaveHandler"/> provided by the CloudSave package.
    /// </summary>
    /// <remarks>
    ///     This class should interface with other services in the DLX in order to safely handle
    ///     saving and loading the game state.
    ///     
    ///     Modify it to suit the needs of the DLX project.
    /// </remarks>
    public class CustomSaveHandler : ExperienceSaveHandler
    {
        [Tooltip("Indicates whether the Load action should be performed automatically once a learner is logged in")]
        public bool LoadOnStart = false;

        protected virtual void OnValidate()
        {
            if (m_AzureSaveSystem == null)
            {
                m_AzureSaveSystem = GetComponent<AzureSaveSystem>();
            }
        }

        public virtual void Start()
        {
            CommandInterpreter.Instance?.Add(new CloudSaveCommand(this));
        }

        /// <summary>
        ///     Receives a username externally (typically from SCORM, may soon be from LTI)
        ///     and updates the 'Blob' name with the specified username.
        /// </summary>
        /// <remarks>
        ///     Instead of simply loading an existing save file on start, DLX may want to prompt
        ///     the user to decide whether they want to load an existing save or start from the beginning.
        /// </remarks>
        /// <param name="username">
        ///     A unique user ID to provide session identification
        /// </param>
        public void HandleLogin(string username)
        {
            Blob = $"DLX_template_{username}";

            if (LoadOnStart)
            {
                Load();
            }
        }
    }
}
