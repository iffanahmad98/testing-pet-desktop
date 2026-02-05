using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TutorialStep
{
    [Header("UI Panel (Opsional)")]
    [Tooltip("Root panel untuk step ini (GameObject yang berisi semua elemen UI tutorial)")]
    public GameObject panelRoot;

    [Tooltip("Tombol untuk lanjut / close tutorial. Boleh kosong kalau pakai trigger lain.")]
    public Button nextButton;

    [Header("Dialog (Opsional)")]
    [Tooltip("Jika true, step ini akan menggunakan popup dialog berurutan, bukan hanya panel statis.")]
    public bool useDialog;

    [Tooltip("Prefab popup dialog untuk step ini. Prefab harus punya komponen TutorialDialogView.")]
    public TutorialDialogView dialogPrefab;

    [Tooltip("Daftar dialog yang akan ditampilkan berurutan untuk step ini.")]
    public List<TutorialDialogLine> dialogLines = new List<TutorialDialogLine>();

    [Header("Pointer (Opsional)")]
    [Tooltip("Jika true, pointer tangan akan muncul di step ini.")]
    public bool usePointer;

    [Tooltip("Jika true, pointer akan otomatis menunjuk ke monster tutorial (mis. Briabit) yang di-spawn oleh TutorialManager.")]
    public bool useTutorialMonsterAsPointerTarget;

    [Tooltip("Target UI yang akan ditunjuk pointer. Jika kosong dan usePointer = true, akan fallback ke nextButton jika ada.")]
    public RectTransform pointerTarget;

    [Tooltip("Offset tambahan dari posisi target (dalam anchoredPosition canvas). Contoh: (40, -40) supaya tangan muncul di samping tombol.")]
    public Vector2 pointerOffset;
}