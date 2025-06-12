using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabController : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public string nameTab;
        public Button tabButton;
    }

    [Header("Tab Configuration")]
    public List<Tab> tabs = new List<Tab>();
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;
    public System.Action<int> OnTabChanged; // Add this at the top


    private int currentTabIndex = 0;

    void Start()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            tabs[i].tabButton.onClick.AddListener(() => OnTabSelected(index));
        }

        // Initialize with the first tab
        OnTabSelected(currentTabIndex);
    }

    public void OnTabSelected(int index)
    {
        currentTabIndex = index;

        for (int i = 0; i < tabs.Count; i++)
        {
            bool isActive = i == index;

            // Optional: Change button visuals
            var colors = tabs[i].tabButton.colors;
            colors.normalColor = isActive ? activeColor : inactiveColor;
            tabs[i].tabButton.colors = colors;
        }
    }
}
