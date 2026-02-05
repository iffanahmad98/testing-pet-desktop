using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public string nameTab;
        public Button tabButton;
        public Sprite activeSprite;
        public Sprite inactiveSprite;
        public Sprite viewportPanel;

        [HideInInspector] public Image buttonImage;
    }

    [Header("Tab Configuration")]
    public List<Tab> tabs = new List<Tab>();
    public Image viewportPanel;
    public Action<int> OnTabChanged;

    private int currentTabIndex = 0;

    void Start()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            // Cache button image reference
            if (tabs[i].tabButton != null)
                tabs[i].buttonImage = tabs[i].tabButton.GetComponent<Image>();

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
            bool isActive = (i == index);

            // Set sprite if image and sprites are assigned
            if (tabs[i].buttonImage != null)
            {
                tabs[i].buttonImage.sprite = isActive ? tabs[i].activeSprite : tabs[i].inactiveSprite;
            }

            // Nonaktifkan image ketika button di-click, aktifkan ketika tidak di-click
            if (tabs[i].buttonImage != null)
            {
                tabs[i].buttonImage.enabled = !isActive;
            }
        }

        // Update viewport panel sprite based on active tab
        if (viewportPanel != null && tabs[index].viewportPanel != null)
        {
            viewportPanel.sprite = tabs[index].viewportPanel;
        }

        OnTabChanged?.Invoke(index);
    }
}
