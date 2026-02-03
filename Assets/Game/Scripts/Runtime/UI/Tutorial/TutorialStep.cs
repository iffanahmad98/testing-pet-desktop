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
}