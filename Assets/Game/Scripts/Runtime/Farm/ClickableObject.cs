using UnityEngine;

public class ClickableObject : MonoBehaviour
{
    public GameObject menuToShow;

    public void ShowMenu()
    {
        if (menuToShow != null)
        {
            menuToShow.SetActive(true);
        }
    }

    public void HideMenu()
    {
        if (menuToShow != null)
        {
            menuToShow.SetActive(false);
        }
    }
}