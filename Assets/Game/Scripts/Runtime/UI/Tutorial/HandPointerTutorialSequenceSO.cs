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
    [Tooltip("Index target di _uiButtonsCache (sesuai urutan UIMainButtons + lainnya). Contoh: 13 berarti pakai button pada index 13 sebagai target.")]
    public int uiButtonIndex = -1;

    [Tooltip("Offset tambahan dari posisi button target (dalam anchoredPosition).")]
    public Vector2 pointerOffset;
}
