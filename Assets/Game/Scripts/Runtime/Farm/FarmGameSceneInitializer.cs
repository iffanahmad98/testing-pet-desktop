using UnityEngine;
using MagicalGarden.Farm;
using System.Collections;

/// <summary>
/// Initializes FarmGame scene based on focus target from previous scene
/// This script should be attached to a GameObject in the FarmGame scene
/// </summary>
public class FarmGameSceneInitializer : MonoBehaviour
{
    [Header("Focus Points")]
    [SerializeField] private Transform farmFocusPoint;
    [SerializeField] private Transform hotelFocusPoint;

    [Header("Camera Settings")]
    [SerializeField] private CameraDragMove cameraController;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float focusZoomSize = 4f;
    [SerializeField] private float focusDuration = 1f;

    [Header("Default Focus")]
    [SerializeField] private bool useFarmAsDefault = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private bool hasInitialized = false; // Track if we've already initialized

    private void Awake()
    {
        // Try to find camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Try to find CameraDragMove if not assigned
        if (cameraController == null && mainCamera != null)
        {
            cameraController = mainCamera.GetComponent<CameraDragMove>();
        }
    }

    private void OnEnable()
    {
        StartCoroutine(InitializeAfterDelay());
    }

    private IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForEndOfFrame();

        // Get the focus target from SceneFocusManager
        SceneFocusManager.FocusTarget target = SceneFocusManager.GetAndClearFocusTarget();

        if (enableDebugLogs)
        {
            Debug.Log($"[FarmGameSceneInitializer] Scene loaded with focus target: {target}");
            Debug.Log($"[FarmGameSceneInitializer] Camera Controller: {(cameraController != null ? "Found" : "NULL")}");
            Debug.Log($"[FarmGameSceneInitializer] Farm Focus Point: {(farmFocusPoint != null ? farmFocusPoint.position.ToString() : "NULL")}");
            Debug.Log($"[FarmGameSceneInitializer] Hotel Focus Point: {(hotelFocusPoint != null ? hotelFocusPoint.position.ToString() : "NULL")}");
        }

        // Initialize camera focus based on target (only once)
        InitializeCameraFocus(target);

        hasInitialized = true; // Mark as initialized
    }

    private void InitializeCameraFocus(SceneFocusManager.FocusTarget target)
    {
        if (cameraController == null)
        {
            Debug.LogError("FarmGameSceneInitializer: CameraDragMove is not assigned!");
            return;
        }

        switch (target)
        {
            case SceneFocusManager.FocusTarget.Farm:
                FocusOnFarm();
                break;

            case SceneFocusManager.FocusTarget.Hotel:
                FocusOnHotel();
                break;

            case SceneFocusManager.FocusTarget.None:
            default:
                // No specific target, use default
                if (useFarmAsDefault)
                {
                    FocusOnFarm();
                    Debug.Log("FarmGameSceneInitializer: No focus target specified, using Farm as default");
                }
                else
                {
                    Debug.Log("FarmGameSceneInitializer: No focus target specified, keeping current camera position");
                }
                break;
        }
    }

    /// <summary>
    /// Focus camera on Farm area
    /// </summary>
    private void FocusOnFarm()
    {
        if (farmFocusPoint == null)
        {
            Debug.LogWarning("[FarmGameSceneInitializer] Farm focus point is not assigned!");
            return;
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[FarmGameSceneInitializer] Focusing on Farm at {farmFocusPoint.position}");
        }

        if (cameraController != null)
        {
            cameraController.FocusOnTarget(farmFocusPoint.position, focusZoomSize, focusDuration, isHotel: false);
        }
        else if (mainCamera != null)
        {
            // Fallback: directly move camera without CameraDragMove
            mainCamera.transform.position = new Vector3(farmFocusPoint.position.x, farmFocusPoint.position.y, mainCamera.transform.position.z);
            Debug.LogWarning("[FarmGameSceneInitializer] CameraDragMove not found, using direct camera positioning");
        }
    }

    /// <summary>
    /// Focus camera on Hotel area
    /// </summary>
    private void FocusOnHotel()
    {
        if (hotelFocusPoint == null)
        {
            Debug.LogWarning("[FarmGameSceneInitializer] Hotel focus point is not assigned!");
            return;
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[FarmGameSceneInitializer] Focusing on Hotel at {hotelFocusPoint.position}");
        }

        if (cameraController != null)
        {
            cameraController.FocusOnTarget(hotelFocusPoint.position, focusZoomSize, focusDuration, isHotel: true);
        }
        else if (mainCamera != null)
        {
            // Fallback: directly move camera without CameraDragMove
            mainCamera.transform.position = new Vector3(hotelFocusPoint.position.x, hotelFocusPoint.position.y, mainCamera.transform.position.z);
            Debug.LogWarning("[FarmGameSceneInitializer] CameraDragMove not found, using direct camera positioning");
        }
    }

    /// <summary>
    /// Public method to manually focus on Farm (can be called from UI buttons in FarmGame scene)
    /// </summary>
    public void ManualFocusOnFarm()
    {
        FocusOnFarm();
    }

    /// <summary>
    /// Public method to manually focus on Hotel (can be called from UI buttons in FarmGame scene)
    /// </summary>
    public void ManualFocusOnHotel()
    {
        FocusOnHotel();
    }
}
