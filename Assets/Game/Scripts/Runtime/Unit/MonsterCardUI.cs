using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterCardUI : MonoBehaviour
{
    public Image monsterImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI priceText;
    public Button selectButton;

    private MonsterDataSO data;
    private MonsterShopManager shopManager;

    public void Setup(MonsterDataSO monsterData, MonsterShopManager manager)
    {
        data = monsterData;
        shopManager = manager;

        nameText.text = data.monsterName;
        typeText.text = data.monType.ToString();
        priceText.text = $"{data.monPrice} coins";
        monsterImage.sprite = data.monsImgs[0];

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => shopManager.ShowMonsterDetail(data));
    }
}
