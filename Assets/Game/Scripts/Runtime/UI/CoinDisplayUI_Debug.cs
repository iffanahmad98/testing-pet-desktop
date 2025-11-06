using UnityEngine;
using TMPro;
using MagicalGarden.Farm;

/// <summary>
/// Version debug dari CoinDisplayUI dengan enhanced logging
/// Replace CoinDisplayUI dengan script ini untuk debug
/// Menggunakan Farm.CoinManager (Instance-based, bukan static)
/// </summary>
public class CoinDisplayUI_Debug : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mainCoinText;
    [SerializeField] private TextMeshProUGUI shopCoinText;

    void Start()
    {
        if (MagicalGarden.Farm.CoinManager.Instance != null)
        {
            Debug.Log($"<color=cyan>[CoinDisplayUI] Start - Initial Coins: {MagicalGarden.Farm.CoinManager.Instance.coins}</color>");
            UpdateCoinText();
        }
        else
        {
            Debug.LogError("<color=red>[CoinDisplayUI] Start - Farm.CoinManager.Instance is NULL!</color>");
        }
    }

    private void OnEnable()
    {
        Debug.Log("<color=cyan>[CoinDisplayUI] OnEnable - Subscribing to CoinManager.OnCoinChanged</color>");

        if (MagicalGarden.Farm.CoinManager.Instance != null)
        {
            MagicalGarden.Farm.CoinManager.Instance.OnCoinChanged += UpdateCoinText;
            UpdateCoinText();
        }
        else
        {
            Debug.LogError("<color=red>[CoinDisplayUI] OnEnable - Farm.CoinManager.Instance is NULL!</color>");
        }
    }

    private void OnDisable()
    {
        Debug.Log("<color=cyan>[CoinDisplayUI] OnDisable - Unsubscribing from CoinManager.OnCoinChanged</color>");

        if (MagicalGarden.Farm.CoinManager.Instance != null)
        {
            MagicalGarden.Farm.CoinManager.Instance.OnCoinChanged -= UpdateCoinText;
        }
    }

    private void UpdateCoinText()
    {
        if (MagicalGarden.Farm.CoinManager.Instance == null)
        {
            Debug.LogError("<color=red>[CoinDisplayUI] UpdateCoinText - Farm.CoinManager.Instance is NULL!</color>");
            return;
        }

        int coins = MagicalGarden.Farm.CoinManager.Instance.coins;
        Debug.Log($"<color=green>[CoinDisplayUI] UpdateCoinText called! Coins: {coins}</color>");

        string displayText = coins >= 10000 ? "9999+" : coins.ToString("N0");

        if (mainCoinText != null)
        {
            mainCoinText.text = displayText;
            Debug.Log($"  ✓ mainCoinText updated: {displayText}");
        }
        else
        {
            Debug.LogError("  ✗ mainCoinText is NULL!");
        }

        if (shopCoinText != null)
        {
            shopCoinText.text = displayText;
            Debug.Log($"  ✓ shopCoinText updated: {displayText}");
        }
        else
        {
            Debug.LogWarning("  ! shopCoinText is NULL (might be ok if no shop UI)");
        }
    }
}
