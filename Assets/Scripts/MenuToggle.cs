using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    public GameObject menuCanvas; 
    private bool isMenuActive = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }
    /// <summary>
    /// Unlocks the cursor and then makes the cursor visible
    /// then the else locks and hides the cursor
    /// </summary>
    public void ToggleMenu()
    {
        isMenuActive = !isMenuActive;
        menuCanvas.SetActive(isMenuActive);

        if (isMenuActive)
        {
            Cursor.lockState = CursorLockMode.None;  
            Cursor.visible = true;                  
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;                   
        }
    }
}
