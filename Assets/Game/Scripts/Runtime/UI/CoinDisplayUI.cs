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

    void Start()
    {
        if (MagicalGarden.Farm.CoinManager.Instance != null)
        {
            UpdateCoinText();
        }
    }

    private void OnEnable()
    {
        if (MagicalGarden.Farm.CoinManager.Instance != null)
        {
            MagicalGarden.Farm.CoinManager.Instance.OnCoinChanged += UpdateCoinText;
            UpdateCoinText();
        }
    }

    private void OnDisable()
    {
        if (MagicalGarden.Farm.CoinManager.Instance != null)
        {
            MagicalGarden.Farm.CoinManager.Instance.OnCoinChanged -= UpdateCoinText;
        }
    }

    private void UpdateCoinText()
    {
        if (MagicalGarden.Farm.CoinManager.Instance == null)
            return;

        int coins = MagicalGarden.Farm.CoinManager.Instance.coins;
        string displayText = coins >= 10000 ? "9999+" : coins.ToString("N0");

        if (mainCoinText != null)
            mainCoinText.text = displayText;

        if (shopCoinText != null)
            shopCoinText.text = displayText;
    }
}