using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class FarmMainUI : MonoBehaviour
{
    public static FarmMainUI instance;
    public Canvas mainCanvas;
    public TMP_Text coinText;
    PlayerConfig playerConfig;
    
    void Awake () {
        instance = this;
    }

    void Start () {
     playerConfig = SaveSystem.PlayerConfig;
     CoinManager.AddCoinChangedRefreshEvent (RefreshCoin);   

     RefreshCoin ();
    }

    public void Show () { // SceneLoadManager.cs, BridgeTeleport.cs
        mainCanvas.gameObject.SetActive (true);
    }

    public void Hide () { // SceneLoadManager.cs, BridgeTeleport.cs
        mainCanvas.gameObject.SetActive (false);
    }
    #region Coin
    void RefreshCoin () {
        coinText.text = playerConfig.coins.ToString ();
    }
    #endregion
}
