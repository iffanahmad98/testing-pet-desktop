using System;
using UnityEngine;

[Serializable]
public class SimpleTutorialPanelStep
{
    [Header("Panel")]
    public GameObject panelRoot;

    [Header("Next Button")]
    public int nextButtonIndex = -1;
    public float nextStepDelay = 0f;

    [Header("Interaction")]
    public bool useLeftClickPetAsNext;
    public bool useRightClickPetAsNext;
    public bool useCoinCollectAsNext;

    [Header("Pointer")]
    public bool usePointer;
    public bool useTutorialMonsterAsPointerTarget;
    public bool useNextButtonAsPointerTarget;
    public Vector2 pointerOffset;

    [Header("Mouse Hint")]
    public bool showRightClickMouseHint;
    public Vector2 rightClickMouseHintOffset;

    [Header("Monster")]
    public bool freezeTutorialMonsterMovement;
    public bool makeTutorialMonsterHungry;
}
