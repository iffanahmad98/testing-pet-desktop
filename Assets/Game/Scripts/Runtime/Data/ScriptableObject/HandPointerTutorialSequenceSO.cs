using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HandPointerTutorialSequence", menuName = "Tutorial/Hand Pointer Sequence", order = 0)]
public class HandPointerTutorialSequenceSO : ScriptableObject
{
    [Tooltip("Daftar step internal untuk sub-tutorial hand pointer.")]
    public List<HandPointerSubStep> steps = new();
}

[Serializable]
public class HandPointerSubStep
{
    public int uiButtonIndex = -1;
    public string ButtonKey;
    public Vector2 pointerOffset;

    [Header("Optional World Target (ClickableObject)")]
    public bool useClickableObjectTarget;

    public string clickableObjectId;

    [Header("Optional GuestItem Button Target")]
    public bool useGuestItemCheckInButton;

    [Header("Optional Hotel Room Target")]
    public bool useHotelRoomTarget;

    public string hotelRoomGuestTypeFilter;

    [Header("Optional Last Assigned Hotel Room Target (Hotel Mode Only)")]
    public bool useLastAssignedHotelRoomTarget;

    [Header("Optional Hotel Gift Target (Hotel Mode Only)")]
    public bool useHotelGiftTarget;

    [Header("Optional Hotel Random Loot Target (Hotel Mode Only)")]
    public bool useHotelRandomLootTarget;

    [Header("Optional Hotel Shop Target (Hotel Mode Only)")]
    public bool useHotelShopTarget;

    [Header("Optional Hotel Facilities Buttons (Hotel Mode Only)")]
    public bool useHotelFacilitiesHireButtonTarget;

    public bool useHotelFacilitiesApplyButtonTarget;
}
