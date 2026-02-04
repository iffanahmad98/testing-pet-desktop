using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClickDebug : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Nama label untuk membedakan log di Console")] public string text;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();

        Debug.Log($"[ButtonClickDebug:{name}] ====== DIAGNOSTIC START ======");

        if (_button == null)
        {
            Debug.LogWarning($"[ButtonClickDebug:{name}] TIDAK menemukan komponen Button di GameObject ini.");
        }
        else
        {
            Debug.Log($"[ButtonClickDebug:{name}] Button ditemukan. interactable={_button.interactable}, enabled={_button.enabled}");

            if (_button.targetGraphic != null)
            {
                Debug.Log(
                    $"[ButtonClickDebug:{name}] targetGraphic='{_button.targetGraphic.gameObject.name}', raycastTarget={_button.targetGraphic.raycastTarget}");
            }
            else
            {
                Debug.LogWarning($"[ButtonClickDebug:{name}] Button tidak punya targetGraphic (Image/Text utama).");
            }

            _button.onClick.AddListener(() =>
            {
                Debug.Log($"[ButtonClickDebug:{name}] {text} BUTTON CLICKED (onClick)");
            });
        }

        var graphics = GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics)
        {
            Debug.Log(
                $"[ButtonClickDebug:{name}] Graphic '{g.gameObject.name}' raycastTarget={g.raycastTarget} activeInHierarchy={g.gameObject.activeInHierarchy}");
        }

        Transform current = transform;
        while (current != null)
        {
            var cg = current.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                bool potentialBlocker = !cg.interactable || !cg.blocksRaycasts || !current.gameObject.activeInHierarchy;
                Debug.Log($"[ButtonClickDebug:{name}] CanvasGroup di '{current.gameObject.name}': activeInHierarchy={current.gameObject.activeInHierarchy}, enabled={cg.enabled}, alpha={cg.alpha}, interactable={cg.interactable}, blocksRaycasts={cg.blocksRaycasts}, POTENTIAL_BLOCKER={potentialBlocker}");
            }

            current = current.parent;
        }
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning($"[ButtonClickDebug:{name}] TIDAK menemukan Canvas di parent chain.");
        }
        else
        {
            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            Debug.Log(
                $"[ButtonClickDebug:{name}] Canvas root='{canvas.gameObject.name}', enabled={canvas.enabled}, activeInHierarchy={canvas.gameObject.activeInHierarchy}, GraphicRaycaster={(raycaster != null ? "ADA" : "TIDAK ADA")}");
        }

        Debug.Log($"[ButtonClickDebug:{name}] ====== DIAGNOSTIC END ======");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[ButtonClickDebug:{name}] {text} CLICKED (IPointerClickHandler)");
    }
}