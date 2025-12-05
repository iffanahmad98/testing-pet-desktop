using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UILayoutResetter : MonoBehaviour
{
    public MonoBehaviour layoutGroup; // Horizontal/Vertical LayoutGroup

    void OnEnable()
    {
        StartCoroutine(ResetLayoutNextFrame());
    }

    public void OnRebuild () { // HotelFacilities
        StartCoroutine(ResetLayoutNextFrame());
    }

    IEnumerator ResetLayoutNextFrame()
    {
        // Matikan LayoutGroup sementara
        layoutGroup.enabled = false;

        // Tunggu 1 frame sampai kamu selesai scale & pos UI
        yield return null;

        // Nyalakan ulang → Unity rebuild layout dengan ukuran baru
        layoutGroup.enabled = true;

        // Force rebuild sebagai tambahan
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}
