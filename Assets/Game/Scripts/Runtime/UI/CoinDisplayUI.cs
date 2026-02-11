using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Display coin UI untuk FarmGame scene
/// Menggunakan Farm.CoinManager (Instance-based, bukan static)
/// </summary>
public class CoinDisplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mainCoinText;
    [SerializeField] private TextMeshProUGUI shopCoinText;
    [SerializeField] private TextMeshProUGUI coinDifferentText;

    private int cacheCoinsValue;
    private int lastCoinDisplay;

    private Tweener tween;

    void Awake()
    {
        ServiceLocator.Register(this);
    }

    void Start()
    {
        //Template Magical Garden
        // if (MagicalGarden.Farm.CoinManager.Instance != null)
        // {
        //     UpdateCoinText();
        // }

        UpdateCoinText();
    }

    private void OnEnable()
    {
        //Template MagicalGarden
        // if (MagicalGarden.Farm.CoinManager.Instance != null)
        // {
        //     MagicalGarden.Farm.CoinManager.Instance.OnCoinChanged += UpdateCoinText;
        //     UpdateCoinText();
        // }

        UpdateCoinText();
    }

    private void OnDisable()
    {
        //Template MagicalGarden
        // if (MagicalGarden.Farm.CoinManager.Instance != null)
        // {
        //     MagicalGarden.Farm.CoinManager.Instance.OnCoinChanged -= UpdateCoinText;
        // }

        UpdateCoinText();
    }

    public void UpdateCoinText()
    {
        //Template MagicalGarden
        // if (MagicalGarden.Farm.CoinManager.Instance == null)
        //     return;

        //int coins = MagicalGarden.Farm.CoinManager.Instance.coins;

        if(tween != null && tween.IsPlaying())
        {
            mainCoinText.text = lastCoinDisplay.ToString();
            shopCoinText.text = lastCoinDisplay.ToString();
            tween.Kill();
        }

        lastCoinDisplay = cacheCoinsValue;  //In case tween not completed

        int coins = CoinManager.Coins;
        cacheCoinsValue = coins;
        Debug.Log($"Update Coin = {coins}");

        int cointDifferent = coins - lastCoinDisplay;

        if (cointDifferent == 0)
            return;
        
        coinDifferentText.gameObject.SetActive(true);
        coinDifferentText.text = cointDifferent > 0 ? $"+{cointDifferent}" : cointDifferent.ToString();
        coinDifferentText.color = cointDifferent > 0 ? Color.green : Color.red;
        coinDifferentText.rectTransform.anchoredPosition = new Vector2(330f, coinDifferentText.rectTransform.anchoredPosition.y);
        coinDifferentText.rectTransform.DOAnchorPosX(345f, 0.2f);
        
        int coinDisplay = lastCoinDisplay;

        if (mainCoinText != null && shopCoinText != null)
        {
            tween = DOTween.To(() => coinDisplay, x => coinDisplay = x, coins, 1f).SetEase(Ease.OutSine).OnUpdate(() =>
            {
                mainCoinText.text = coinDisplay.ToString();
                shopCoinText.text = coinDisplay.ToString();
            }).OnComplete(() =>
            {
                string displayText = coins >= 1000000 ? "999999+" : coins.ToString();

                mainCoinText.text = displayText;
                shopCoinText.text = displayText;
                coinDifferentText.gameObject.SetActive(false);
                lastCoinDisplay = coins;
            });
        }
    }
}