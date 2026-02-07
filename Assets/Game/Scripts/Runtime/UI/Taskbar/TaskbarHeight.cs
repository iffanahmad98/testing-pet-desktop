/*
using System;
using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_STANDALONE_WIN
using System.Windows.Forms;
#endif
public class TaskbarHeight
{
    public static int GetTaskbarHeight()
    {
        // Resolusi monitor penuh
        int fullHeight = Screen.currentResolution.height;

        // Area kerja tanpa taskbar
        int workingHeight = Screen.PrimaryScreen.WorkingArea.Height;

        // Selisih = tinggi taskbar (jika di bawah / atas)
        return fullHeight - workingHeight;
    }
}
*/