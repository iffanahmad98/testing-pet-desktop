using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class HotelMainUI : MonoBehaviour
{
    public static HotelMainUI instance;
    public Canvas mainCanvas;
    PlayerConfig playerConfig;
    [SerializeField] GoToPetScene gotoPetScene;
    [Header ("Display UI")]
    public TMP_Text coinText;
    public TMP_Text goldenTicketText;
    public TMP_Text hotelGiftText;
    public TMP_Text eggText;
    public TMP_Text hotelRoomText;
    public Button backToPlainsButton;
    public Button muteButton;
    [SerializeField] Sprite onMute, offMute;
    void Awake () {
        instance = this;
    }

    void Start () {
     playerConfig = SaveSystem.PlayerConfig;
     backToPlainsButton.onClick.AddListener (BackToPlains);
     muteButton.onClick.AddListener (ClickMuteButton);
     CoinManager.AddCoinChangedRefreshEvent (RefreshCoin);   
     DebugTimeController.instance.AddLateDebuggingEvent (RefreshGoldenTicket);
     DebugTimeController.instance.AddLateDebuggingEvent (RefreshEgg);
     DebugTimeController.instance.AddLateDebuggingEvent (RefreshHotelRoom);
     RefreshCoin ();
     RefreshGoldenTicket ();
     RefreshHotelGift ();
     RefreshEgg ();
     RefreshHotelRoom ();
    }
    
    public void Show () { // SceneLoadManager.cs, BridgeTeleport.cs
        Debug.Log ("Show Hotel UI");
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
    #region GoldenTicket
    void RefreshGoldenTicket () {
        goldenTicketText.text = playerConfig.goldenTicket.ToString ();
    }
    #endregion
    #region HotelGift
    void RefreshHotelGift () {
        hotelGiftText.text = playerConfig.hotelGift.ToString ();
    }
    #endregion
    #region Egg
    public void RefreshEgg () {
        eggText.text = playerConfig.normalEgg.ToString () + " / " + playerConfig.rareEgg.ToString ();
    }
    #endregion
    #region HotelRoom

    public void RefreshHotelRoom () { // HotelController.cs, HotelManager.cs (Setelah load Coroutine)
       hotelRoomText.text = MagicalGarden.Manager.HotelManager.Instance.GetTotalHotelControllerOccupied ().ToString () + " / " + playerConfig.listIdHotelOpen.Count.ToString ();
    }

    #endregion
    #region Back
    public void BackToPlains () {
        MonsterManager.instance.audio.StopAllSFX();
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
