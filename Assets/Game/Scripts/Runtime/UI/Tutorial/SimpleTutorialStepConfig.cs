using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Simple Tutorial Step Config", fileName = "SimpleTutorialStepConfig")]
public class SimpleTutorialStepConfig : ScriptableObject
{
    [Header("Next Button")]
    public int nextButtonIndex = -1;
    public float nextStepDelay = 0f;

    [Header("Timing")]
    public float minNextClickDelay = 0f;

    [Header("Interaction")]
    public bool useLeftClickPetAsNext;
    public bool useRightClickPetAsNext;
    public bool useCoinCollectAsNext;
    public bool useFoodDropAsNext;
    public bool usePoopCleanAsNext;
    public int requiredFoodDropCount = 1;

    [Header("Pointer")]
    public bool usePointer;
    public bool useTutorialMonsterAsPointerTarget;
    public bool useNextButtonAsPointerTarget;
    public Vector2 pointerOffset;

    [Header("UI Hand Pointer")]
    public bool useUIManagerButtonHandPointer;
    public HandPointerTutorialSequenceSO handPointerSequence;

    [Header("Mouse Hint")]
    public bool showRightClickMouseHint;
    public Vector2 rightClickMouseHintOffset;

    [Header("Monster")]
    public bool freezeTutorialMonsterMovement;
    public bool makeTutorialMonsterHungry;
    [Range(0f, 100f)] public float hungryReduceAmount = 50f;
    public bool dropPoopOnStepStart;

    [Header("Monster Position")]
    public bool moveTutorialMonsterToTarget;
    [Tooltip("ID marker di scene (TutorialMonsterTargetMarker.id) yang akan dipakai sebagai posisi monster tutorial.")]
    public string monsterTargetId;

    [Header("Monster Info")]
    public bool showMonsterInfoOnStepStart;

    [Header("Food Drop Timing")]
    public float minFoodDropDelay = 5f;
}
