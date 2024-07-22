using VARLab.CloudSave;
using VARLab.DeveloperTools;

namespace VARLab.DLX
{

    /// <summary>
    ///     Extends the <see cref="ExperienceSaveHandler"/> provided by the 
    ///     CloudSave package in iorder
    /// </summary>
    public class CustomSaveHandler : ExperienceSaveHandler
    {
        public bool LoadOnStart = true;

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
