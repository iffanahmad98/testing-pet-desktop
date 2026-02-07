using UnityEngine;
using System.Runtime.InteropServices;

public class MinimizeWindow : MonoBehaviour
{
    #if UNITY_STANDALONE_WIN
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(System.IntPtr hwnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    const int SW_MINIMIZE = 6;
    #endif

    public void Minimize()
    {
       // Debug.LogError ("Minimize Window");
        #if UNITY_STANDALONE_WIN
        Screen.fullScreen = false;
        ShowWindow(GetActiveWindow(), SW_MINIMIZE);
        #endif
    }
}
