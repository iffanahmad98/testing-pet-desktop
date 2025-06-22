using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaResultPanel : MonoBehaviour
{
    public GameObject root;
    public Image monsterImage;
    public TextMeshProUGUI monsterNameText;
    public TextMeshProUGUI rarityText;
    public GameObject confettiEffect;
    public Button backButton;
    public Button rollAgainButton;

    public void Show(MonsterDataSO monster, System.Action onRollAgain)
    {
        root.SetActive(true);
        monsterImage.sprite = monster.monsIconImg[0];
        monsterNameText.text = monster.monsterName;
        rarityText.text = monster.monType.ToString().ToUpper();

        if (confettiEffect != null) confettiEffect.SetActive(true);

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(() => root.SetActive(false));

        rollAgainButton.onClick.RemoveAllListeners();
        rollAgainButton.onClick.AddListener(() =>
        {
            root.SetActive(false);
            onRollAgain?.Invoke();
        });
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}
