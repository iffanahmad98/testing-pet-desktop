using UnityEngine;
using TMPro;
using MagicalGarden.Farm;

/// <summary>
/// Display coin UI untuk FarmGame scene
/// Menggunakan Farm.CoinManager (Instance-based, bukan static)
/// </summary>
public class CoinDisplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mainCoinText;
    [SerializeField] private TextMeshProUGUI shopCoinText;

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

        int coins = CoinManager.Coins;
        Debug.Log($"Update Coin = {coins}");
        
        string displayText = coins >= 10000 ? "9999+" : coins.ToString();

        if (mainCoinText != null)
            mainCoinText.text = displayText;

        if (shopCoinText != null)
            shopCoinText.text = displayText;
    }
}