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
    [Tooltip("Jika true, step ini akan menarget ClickableObject di scene berdasarkan clickableObjectId, bukan UI Button.")]
    public bool useClickableObjectTarget;

    [Tooltip("TutorialId dari ClickableObject di scene yang ingin dijadikan target hand pointer.")]
    public string clickableObjectId;

    [Header("Optional GuestItem Button Target")]
    [Tooltip("Jika true, step ini akan menarget check-in button dari GuestItem dengan guest name tertentu.")]
    public bool useGuestItemCheckInButton;

    [Tooltip("Nama guest (guestName) yang GuestItem-nya akan ditarget. Contoh: 'Koaliarus'.")]
    public string guestNameFilter;

    [Tooltip("Type guest (Fire, Water, Earth, dll.) untuk filter lebih spesifik. Kosongkan untuk pilih random dari nama yang sama.")]
    public string guestTypeFilter;

    [Header("Optional Hotel Room Target")]
    [Tooltip("Jika true, step ini akan menarget HotelController yang sedang ada guest-nya (IsOccupied=true), dipilih secara random.")]
    public bool useHotelRoomTarget;

    [Tooltip("(Opsional) Filter berdasarkan type guest di hotel room. Kosongkan untuk pilih random dari semua occupied room.")]
    public string hotelRoomGuestTypeFilter;
}
