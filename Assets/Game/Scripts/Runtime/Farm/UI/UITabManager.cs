using UnityEngine;
using UnityEngine.UI;

public class UITabManager : MonoBehaviour
{
    [Header("Panels")]
    public Tab[] tabs;
    [Header("Buttons (Optional)")]
    public Button[] tabButtons;
    private int currentIndex = 0;
    [SerializeField] MagicalGarden.Inventory.InventoryUISendToPlains inventoryUISendToPlains;
    public event System.Action<int> clickEvent;
    void Start()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i; // capture index for lambda
            tabs[i].button.onClick.AddListener(() => OnTabClicked(index));
        }
        inventoryUISendToPlains.StartPlains(this);
        OnTabClicked(0); // buka tab pertama secara default
    }

    public void OnTabClicked(int index)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            bool isActive = (i == index);
            if (isActive)
            {
                tabs[i].activePanel.GetComponent<Image>().enabled = true;
                tabs[i].idlePanel.SetActive(false);
                tabs[i].activePanel.transform.SetAsLastSibling();
            }
            else
            {
                tabs[i].activePanel.GetComponent<Image>().enabled = false;
                tabs[i].idlePanel.SetActive(true);
            }
            // tabs[i].panel.SetActive(isActive);
        }

        currentIndex = index;
        clickEvent?.Invoke(currentIndex);
    }

    public void AddEventClick(System.Action<int> eventValue)
    {
        clickEvent += eventValue;
    }
}

[System.Serializable]
public class Tab
{
    public Button button;
    public Image tabImage;
    public Sprite activeSprite;
    public Sprite idleSprite;
    public GameObject activePanel;
    public GameObject idlePanel;
}