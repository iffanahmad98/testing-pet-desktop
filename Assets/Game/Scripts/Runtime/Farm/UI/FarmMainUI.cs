using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class FarmMainUI : MonoBehaviour
{
    public static FarmMainUI instance;
    public Canvas mainCanvas;
    PlayerConfig playerConfig;
    [SerializeField] GoToPetScene gotoPetScene;
    [Header ("Display UI")]
    public Button backToPlainsButton;
    public Button muteButton;
    public TMP_Text coinText;
    [SerializeField] Sprite onMute, offMute;

    void Awake () {
        instance = this;
    }

    void Start () {
     playerConfig = SaveSystem.PlayerConfig;
     backToPlainsButton.onClick.AddListener (BackToPlains);
     muteButton.onClick.AddListener (ClickMuteButton);
     CoinManager.AddCoinChangedRefreshEvent (RefreshCoin);   

     RefreshCoin ();
    }

    public void Show () { // SceneLoadManager.cs, BridgeTeleport.cs
        mainCanvas.gameObject.SetActive (true);
        RefreshSpriteMute ();
    }

    public void Hide () { // SceneLoadManager.cs, BridgeTeleport.cs
        mainCanvas.gameObject.SetActive (false);
    }
    #region Coin
    void RefreshCoin () {
        coinText.text = playerConfig.coins.ToString ();
    }
    #endregion
    #region Back
    public void BackToPlains () {
        gotoPetScene.PetScene ();
    }

    #endregion
    #region Mute
    public void ClickMuteButton () {
        ServiceLocator.Get<AudioSettingsManager> ().SetMute ();
        RefreshSpriteMute ();
    }

    void RefreshSpriteMute () {
        bool isMuted = ServiceLocator.Get<AudioSettingsManager> ().GetMuted ();
        if (isMuted) {
            muteButton.image.sprite = onMute;
        } else {
            muteButton.image.sprite = offMute;
        }
    } 
    #endregion
}
