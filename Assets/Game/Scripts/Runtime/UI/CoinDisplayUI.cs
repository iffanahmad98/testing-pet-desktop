// UI code (e.g. CoinDisplayUI.cs)
using UnityEngine;
using TMPro;

public class CoinDisplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mainCoinText;
    [SerializeField] private TextMeshProUGUI shopCoinText;

    // private void Awake()
    // {
    //     CoinManager.OnCoinChanged += UpdateCoinText;
    //     UpdateCoinText(CoinManager.Coins);
    // }

    void Start()
    {
        UpdateCoinText(CoinManager.Coins);
    }
    
    private void OnEnable()
    {
        CoinManager.OnCoinChanged += UpdateCoinText;
        UpdateCoinText(CoinManager.Coins);
    }

    private void OnDisable()
    {
        CoinManager.OnCoinChanged -= UpdateCoinText;
    }

    private void UpdateCoinText(int coins)
    {
        string displayText = coins >= 10000 ? "9999+" : coins.ToString("N0");
        mainCoinText.text = displayText;
        shopCoinText.text = displayText;
    }
}