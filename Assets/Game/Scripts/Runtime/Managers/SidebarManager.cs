using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SidebarManager : MonoBehaviour
{
    [System.Serializable]
    public class SidebarPanelLink
    {
        public Button sidebarButton;
        public GameObject linkedPanel;
    }

    public List<SidebarPanelLink> sidebarLinks;
    private GameObject currentPanel;

    void Start()
    {
        foreach (var link in sidebarLinks)
        {
            link.sidebarButton.onClick.AddListener(() => ShowPanel(link.linkedPanel));
        }

        if (sidebarLinks.Count > 0)
            ShowPanel(sidebarLinks[0].linkedPanel);
    }

    void ShowPanel(GameObject panel)
    {
        if (currentPanel != null)
            currentPanel.SetActive(false);

        panel.SetActive(true);
        currentPanel = panel;
    }
}
