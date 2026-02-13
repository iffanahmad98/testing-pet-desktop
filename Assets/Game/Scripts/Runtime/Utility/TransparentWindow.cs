#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class TransparentWindow : MonoBehaviour
{
    #region Windows API Constants
    private const int GWL_EXSTYLE = -20;
    private const uint WS_EX_LAYERED = 0x00080000;
    private const uint WS_EX_TRANSPARENT = 0x00000020;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const int SW_RESTORE = 9;
    private static readonly IntPtr HWND_TOPMOST = new(-1);
    #endregion

    #region Windows API Imports
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetActiveWindow();

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    #endregion

    #region Structures
    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight;
    }
    #endregion

    #region Private Fields
    [SerializeField] private bool enableTopMost = false; // Change this to false
    [SerializeField] private bool enableClickThrough = true; // Keep this true

    private EventSystem eventSystem;
    private bool wasOverUI = false;
    private IntPtr windowHandle = IntPtr.Zero;
    private Camera mainCamera;
    private bool isMinimizing = false;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (!IsWindowsPlatform())
        {
            Debug.LogWarning("TransparentWindow is only supported on Windows platforms.");
            enabled = false;
            return;
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found for TransparentWindow!");
            enabled = false;
            return;
        }
        
        // Register in ServiceLocator
        ServiceLocator.Register(this);
    }

    void OnDestroy()
    {
        ServiceLocator.Unregister<TransparentWindow>();
    }

    private void Start()
    {
        if (!enabled) return;

        InitializeWindow();
        SetupCameraTransparency();

        eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogWarning("No EventSystem found. UI interaction detection will not work.");
        }
    }

    private void Update()
    {
        if (!enableClickThrough || eventSystem == null) return;

        HandleUIInteraction();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // Don't restore properties if we're in the process of minimizing
        if (isMinimizing) return;
        
        if (hasFocus)
        {
            // Restore window settings when focus is regained
            SetTransparentWindow();
            if (enableTopMost)
            {
                SetTopMost();
            }
        }
        else if (enableTopMost)
        {
            MaintainTopMostState();
        }
    }
    #endregion

    #region Private Methods
    private bool IsWindowsPlatform()
    {
        return Application.platform == RuntimePlatform.WindowsPlayer ||
               Application.platform == RuntimePlatform.WindowsEditor;
    }

    private void InitializeWindow()
    {
        try
        {
            windowHandle = GetActiveWindow();
            if (windowHandle == IntPtr.Zero)
            {
                Debug.LogError("Failed to get window handle");
                return;
            }

            if (enableTopMost)
            {
                SetTopMost();
            }

            SetTransparentWindow();
            SetTransparentMargins();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize transparent window: {ex.Message}");
        }
    }

    private void SetupCameraTransparency()
    {
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = Color.clear;
        mainCamera.allowHDR = false;
        mainCamera.allowMSAA = false;
    }

    private void SetTopMost()
    {
        SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
    }

    private void SetTransparentWindow()
    {
        uint style = enableClickThrough ? (WS_EX_LAYERED | WS_EX_TRANSPARENT) : WS_EX_LAYERED;
        SetWindowLong(windowHandle, GWL_EXSTYLE, style);
    }

    private void SetTransparentMargins()
    {
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(windowHandle, ref margins);
    }

    private void HandleUIInteraction()
    {
        bool isOverUI = eventSystem.IsPointerOverGameObject();

        if (isOverUI != wasOverUI)
        {
            SetClickthrough(!isOverUI);
            wasOverUI = isOverUI;
        }
    }

    private void SetClickthrough(bool clickthrough)
    {
        if (windowHandle == IntPtr.Zero) return;

        try
        {
            uint style = clickthrough ? (WS_EX_LAYERED | WS_EX_TRANSPARENT) : WS_EX_LAYERED;
            SetWindowLong(windowHandle, GWL_EXSTYLE, style);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to set clickthrough: {ex.Message}");
        }
    }

    private void MaintainTopMostState()
    {
        if (windowHandle == IntPtr.Zero) return;

        try
        {
            ShowWindow(windowHandle, SW_RESTORE);
            SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0,
                        SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to maintain topmost state: {ex.Message}");
        }
    }
    #endregion

    #region Public Methods
    public void SetTopMostEnabled(bool enabled)
    {
        enableTopMost = enabled;
        if (enabled && windowHandle != IntPtr.Zero)
        {
            SetTopMost();
        }
    }

    public void MinimizeWindow()
    {
        if (windowHandle == IntPtr.Zero) return;

        try
        {
            isMinimizing = true;
            
            // Remove topmost flag - use HWND_NOTOPMOST (-2)
            SetWindowPos(windowHandle, new IntPtr(-2), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

            // Remove transparent and layered styles temporarily
            SetWindowLong(windowHandle, GWL_EXSTYLE, 0);

            // Now minimize the window
            ShowWindow(windowHandle, 6); // SW_MINIMIZE
            
            // Reset the flag after a delay
            Invoke(nameof(ResetMinimizeFlag), 1f);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to minimize window: {ex.Message}");
            isMinimizing = false;
        }
    }

    private void ResetMinimizeFlag()
    {
        isMinimizing = false;
    }

    public void RestoreWindow()
    {
        if (windowHandle == IntPtr.Zero) return;

        try
        {
            // Restore the window
            ShowWindow(windowHandle, SW_RESTORE);

            // Re-apply transparent window settings
            SetTransparentWindow();
            SetTransparentMargins();

            // Re-apply topmost if enabled (this also helps with focus)
            if (enableTopMost)
            {
                SetTopMost();
            }
            
            // Force focus to the window
            SetForegroundWindow(windowHandle);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to restore window: {ex.Message}");
        }
    }

    public void SetTopMostMode(bool enable)
    {
        enableTopMost = enable;
        
        if (windowHandle == IntPtr.Zero) return;
        
        try
        {
            IntPtr insertAfter = enable ? HWND_TOPMOST : new IntPtr(-2); // -2 is HWND_NOTOPMOST
            SetWindowPos(windowHandle, insertAfter, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to set topmost mode: {ex.Message}");
        }
    }
    #endregion
    #region WindowAdjustableCanvas
    public void ChangeEnableTopMost (bool value) {
        enableTopMost = value;
    }
    #endregion
}
#endif