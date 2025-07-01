using UnityEngine;
using UnityEngine.UI;

public class UITabManager : MonoBehaviour
{
    [Header("Panels")]
    public Tab[] tabs;
    [Header("Buttons (Optional)")]
    public Button[] tabButtons;
    private int currentIndex = 0;
    void Start()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i; // capture index for lambda
            tabs[i].button.onClick.AddListener(() => OnTabClicked(index));
        }

        OnTabClicked(0); // buka tab pertama secara default
    }

    public void OnTabClicked(int index)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            bool isActive = (i == index);
            tabs[i].tabImage.sprite = isActive ? tabs[i].activeSprite : tabs[i].idleSprite;
            if (isActive)
            {
                tabs[i].panel.transform.SetAsLastSibling();
            }
            // tabs[i].panel.SetActive(isActive);
        }

        currentIndex = index;
    }
}

[System.Serializable]
public class Tab
{
    public Button button;
    public Image tabImage;
    public Sprite activeSprite;
    public Sprite idleSprite;
    public GameObject panel;
}