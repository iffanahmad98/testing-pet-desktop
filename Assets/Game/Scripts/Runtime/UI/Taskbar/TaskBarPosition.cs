using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class TaskbarPosition
{
    private const int ABM_GETTASKBARPOS = 0x00000005;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left, top, right, bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public RECT rc;
        public int lParam;
    }

    [DllImport("shell32.dll")]
    private static extern uint SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

    public static string GetTaskbarPosition()
    {
        APPBARDATA data = new APPBARDATA();
        data.cbSize = Marshal.SizeOf(data);

        SHAppBarMessage(ABM_GETTASKBARPOS, ref data);

        switch (data.uEdge)
        {
            case 0: return "Left";
            case 1: return "Top";
            case 2: return "Right";
            case 3: return "Bottom";
            default: return "Unknown";
        }
    }
}
