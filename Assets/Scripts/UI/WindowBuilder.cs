using UnityEngine.UIElements;

namespace VARLAB.DLX 
{
    public class WindowBuilder : MenuController
    {
        public const string DefaultButtonId = "Button";

        public string CloseButtonId;

        private Button closeButton;

        public override void Initialize()
        {
            // Checking to see if the UI Document has a template container with the name inputted to close the UI
            TemplateContainer templateContainer = Root.Q<TemplateContainer>(CloseButtonId);

            if (templateContainer != null) 
            {
                closeButton = templateContainer.Q<Button>(DefaultButtonId);
            }
            else 
            {
                closeButton = Root.Q<Button>(CloseButtonId);
            }

            closeButton.clicked += Close;
        }
    }
}