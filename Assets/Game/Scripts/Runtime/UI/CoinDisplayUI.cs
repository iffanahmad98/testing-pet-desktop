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
    
    // private void OnEnable()
    // {
    //     CoinManager.OnCoinChanged += UpdateCoinText;
    //     UpdateCoinText(CoinManager.Coins);
    // }

    private void OnDisable()
    {
        CoinManager.OnCoinChanged -= UpdateCoinText;
    }

    private void UpdateCoinText(int coins)
    {
        mainCoinText.text = coins.ToString("N0");
        shopCoinText.text = coins.ToString("N0");
    }
}