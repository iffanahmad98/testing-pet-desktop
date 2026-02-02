using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class HotelMainUI : MonoBehaviour
{
    public static HotelMainUI instance;
    public Canvas mainCanvas;
    PlayerConfig playerConfig;

    [Header ("Display UI")]
    public TMP_Text coinText;
    public TMP_Text goldenTicketText;
    public TMP_Text hotelGiftText;
    public TMP_Text eggText;
    public TMP_Text hotelRoomText;

    void Awake () {
        instance = this;
    }

    void Start () {
     playerConfig = SaveSystem.PlayerConfig;
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

    public void RefreshHotelRoom () { // HotelController.cs
       hotelRoomText.text = MagicalGarden.Manager.HotelManager.Instance.GetTotalHotelControllerOccupied ().ToString () + " / " + playerConfig.GetTotalIdHotel ().ToString ();
    }

    #endregion
}
