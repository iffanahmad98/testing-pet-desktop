using System;
using UnityEngine;

[Serializable]
public class TutorialDialogLine
{
    [Tooltip("Nama speaker / karakter. Boleh dikosongkan kalau tidak perlu.")]
    public string speakerName;

    [TextArea]
    [Tooltip("Teks dialog yang akan ditampilkan.")]
    public string text;
}