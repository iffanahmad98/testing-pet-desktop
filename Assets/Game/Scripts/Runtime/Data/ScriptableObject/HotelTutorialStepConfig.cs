using UnityEngine;

[CreateAssetMenu(
    menuName = "Tutorial/Hotel Tutorial Step Config",
    fileName = "HotelTutorialStepConfig"
)]
public class HotelTutorialStepConfig : ScriptableObject
{
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
}
