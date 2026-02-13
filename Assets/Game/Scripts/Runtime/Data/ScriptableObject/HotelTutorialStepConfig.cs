using UnityEngine;

[CreateAssetMenu(
    menuName = "Tutorial/Hotel Tutorial Step Config",
    fileName = "HotelTutorialStepConfig"
)]
public class HotelTutorialStepConfig : ScriptableObject
{
    [Header("Next Button Timing")]
    public float minNextClickDelay = 0f;

    [Header("Panel Sorting")]
    public bool bringPanelRootToFront;

    [Header("Pointer")]
    public bool usePointer;
    public bool useNextButtonAsPointerTarget;
    public bool useClickableObjectAsPointerTarget;
    public Vector2 pointerOffset;
    public string nextButtonName;

    [Header("ClickableObject Target (Scene)")]
    public bool useClickableObjectAsNext;
    public string clickableObjectId;

    [Header("Hand Pointer Sub Tutorial")]
    public HandPointerTutorialSequenceSO handPointerSequence;

    [Header("Hotel Room Guest Focus")]
    public bool focusCameraOnLastCheckedInGuestRoom;
    public float cameraFocusDuration = 1.5f;
    public Vector2 cameraFocusOffset;

    [Header("Pet Guest CheckIn Focus")]
    public bool focusCameraOnLastCheckedInGuest;
    public Vector2 guestCameraFocusOffset;

    [Header("Hotel Loot Focus (Marked Room)")]
    public bool focusCameraOnHotelLootFromMarkedRoom;
    public Vector2 hotelLootCameraFocusOffset;

    [Header("Hotel Shop Focus")]
    public bool focusCameraOnHotelShop;
    public Vector2 hotelShopCameraFocusOffset;

    [Header("Hotel Gift")]
    public bool spawnTutorialGiftFromLastAssignedHotelRoom;

    public bool accelerateGiftOnClean;

    public float giftOnCleanBoostDuration = 10f;

    [Header("Auto Wait To Next Step")]
    public bool waitBeforeNextStep;

    public float waitBeforeNextDuration = 0f;
}
