using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterDetailPanel : MonoBehaviour
{
    public Image monsterImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI stageText;
    public Slider fullnessSlider;
    public Slider happinessSlider;
    public Slider evolvingSlider;
    public Button sellButton;
    public TextMeshProUGUI sellPriceText;
    public TextMeshProUGUI earningRateText;

    private MonsterDataSO currentData;

    public void SetData(MonsterDataSO data)
    {
        currentData = data;

        nameText.text = data.monsterName;
        typeText.text = $"Type: {data.monType}";
        fullnessSlider.value = data.baseHunger / 100f;
        happinessSlider.value = data.baseHappiness / 100f;
        evolvingSlider.value = data.evolutionLevel / 3f; // Adjust max stage
        stageText.text = $"Stage {data.evolutionLevel + 1}";
        monsterImage.sprite = data.monsIconImg[data.isEvolved ? 1 : 0];
        sellPriceText.text = $"{data.monsterPrice / 2} coins";
        earningRateText.text = $"{(data.baseHappiness * 0.01f):0.0} coin/min";
    }

    public void SetVisible(bool show)
    {
        gameObject.SetActive(show);
    }
}
