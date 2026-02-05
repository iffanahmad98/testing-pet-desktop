using System;
using UnityEngine;

[Serializable]
public class SimpleTutorialPanelStep
{
    [Tooltip("Root panel untuk step tutorial sederhana ini.")]
    public GameObject panelRoot;

    [Tooltip("Index button Next di cache UIManager (TutorialManager). -1 = tidak pakai button.")]
    public int nextButtonIndex = -1;

    [Header("Timing")]
    [Tooltip("Jeda (detik) setelah Next di-trigger sebelum pindah ke step berikutnya.")]
    public float nextStepDelay = 0f;

    [Header("Interaction (Opsional)")]
    [Tooltip("Jika true, step ini akan lanjut ketika player left-click pada pet tutorial (mis. Briabit).")]
    public bool useLeftClickPetAsNext;

    [Tooltip("Jika true, step ini akan lanjut ketika player right-click pada pet tutorial (mis. Briabit).")]
    public bool useRightClickPetAsNext;

    [Header("Pointer (Opsional)")]
    [Tooltip("Jika true, pointer tangan akan muncul di step sederhana ini.")]
    public bool usePointer;

    [Tooltip("Jika true, pointer akan otomatis menunjuk ke monster tutorial (mis. Briabit) yang di-spawn oleh TutorialManager.")]
    public bool useTutorialMonsterAsPointerTarget;

    [Tooltip("Jika true, pointer akan menunjuk ke Next Button yang dikonfigurasi untuk step ini (berdasarkan nextButtonIndex).")]
    public bool useNextButtonAsPointerTarget;

    [Tooltip("Offset tambahan dari posisi target (biasanya tombol Next) dalam anchoredPosition canvas.")]
    public Vector2 pointerOffset;

    [Header("Mouse Hint (Opsional)")]
    [Tooltip("Jika true, munculkan gambar hint mouse right-click di dekat pet untuk step ini.")]
    public bool showRightClickMouseHint;

    [Tooltip("Offset posisi hint mouse dari pet (anchoredPosition canvas).")]
    public Vector2 rightClickMouseHintOffset;
}
