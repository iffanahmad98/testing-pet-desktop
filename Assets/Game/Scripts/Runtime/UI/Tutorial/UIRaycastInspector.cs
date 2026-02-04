using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIRaycastInspector : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current == null)
            {
                Debug.LogWarning("[UIRaycastInspector] EventSystem.current is null");
                return;
            }

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            Debug.Log($"[UIRaycastInspector] Click at {eventData.position}, hits: {results.Count}");
            foreach (var r in results)
            {
                Debug.Log($"  Hit: {r.gameObject.name} (sortingLayer={r.sortingLayer}, order={r.sortingOrder})");
            }
        }
    }
}
