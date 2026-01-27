using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SidebarManager : MonoBehaviour
{
    [System.Serializable]
    public class SidebarPanelLink
    {
        public Button sidebarButton;
        public GameObject linkedPanel;
        public Sprite activeSprite;
        public Sprite inactiveSprite;
        [HideInInspector] public Image buttonImage;
    }

    public List<SidebarPanelLink> sidebarLinks;
    public GameObject currentPanel;
    private SidebarPanelLink currentLink;

    void Start()
    {
        ServiceLocator.Register<SidebarManager>(this);
        foreach (var link in sidebarLinks)
        {
            link.buttonImage = link.sidebarButton.GetComponent<Image>();
            link.sidebarButton.onClick.AddListener(() => ShowPanel(link));
        }

        if (sidebarLinks.Count > 0)
            ShowPanel(sidebarLinks[0]);
    }

    public void ShowPanel(SidebarPanelLink linkToShow)
    {
        // Deactivate current panel
        if (currentPanel != null)
            currentPanel.SetActive(false);

        // Set old button sprite to inactive
        if (currentLink != null && currentLink.buttonImage != null)
            currentLink.buttonImage.sprite = currentLink.inactiveSprite;

        // Activate new panel
        linkToShow.linkedPanel?.SetActive(true);
        currentPanel = linkToShow.linkedPanel;

        // Set new button sprite to active
        if (linkToShow.buttonImage != null)
            linkToShow.buttonImage.sprite = linkToShow.activeSprite;

        currentLink = linkToShow;
    }

    /*    public void test()
        {

            // Set old button sprite to inactive
            if (currentLink != null && currentLink.buttonImage != null)
                currentLink.buttonImage.sprite = currentLink.inactiveSprite;

            // Activate new panel
            sidebarLinks[2].linkedPanel?.SetActive(true);
            currentPanel = sidebarLinks[2].linkedPanel;

            // Set new button sprite to active
            if (sidebarLinks[2].buttonImage != null)
                sidebarLinks[2].buttonImage.sprite = sidebarLinks[2].activeSprite;

            currentLink = sidebarLinks[2];
        }*/
}
