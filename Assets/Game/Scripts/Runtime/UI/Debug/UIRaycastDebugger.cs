using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRaycastDebugger : MonoBehaviour
{
    [SerializeField] private bool logHover = true;
    [SerializeField] private bool logClick = true;

    private string _lastHoverName = "";

    private void Update()
    {
        if (EventSystem.current == null)
            return;

        var data = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        if (logHover)
            LogHover(results);

        if (logClick && Input.GetMouseButtonDown(0))
            LogClick(results);
    }

    private void LogHover(List<RaycastResult> results)
    {
        string currentName = results.Count > 0 ? results[0].gameObject.name : "<none>";

        if (currentName != _lastHoverName)
        {
            _lastHoverName = currentName;
            Debug.Log($"[UIRaycastDebugger] Pointer di atas: {currentName}");
        }
    }

    private void LogClick(List<RaycastResult> results)
    {
        if (results.Count == 0)
        {
            Debug.Log("[UIRaycastDebugger] Klik: tidak mengenai UI apapun");
            return;
        }

        Debug.Log("[UIRaycastDebugger] Klik mengenai (urutan atas → bawah):");
        for (int i = 0; i < results.Count; i++)
        {
            var r = results[i];
            Debug.Log($"{i}. {r.gameObject.name} (sortingOrder={r.sortingOrder}, depth={r.depth})");
        }
    }
}
