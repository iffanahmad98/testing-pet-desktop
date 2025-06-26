using UnityEngine;
using UnityEngine.UI;

public class UITabManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject[] tabPanels;

    [Header("Buttons (Optional)")]
    public Button[] tabButtons;

    private int currentTab = 0;

    void Start()
    {
        ShowTab(currentTab); // Tampilkan tab pertama
    }

    public void ShowTab(int tabIndex)
    {
        for (int i = 0; i < tabPanels.Length; i++)
        {
            tabPanels[i].SetActive(i == tabIndex);
        }

        currentTab = tabIndex;
    }
}