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
}
