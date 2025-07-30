#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class AddHighlightTools
{
    // -------------- open scene -----------------
    [MenuItem("Tools/UI/Add Highlight ► Current Scene")]
    static void AddToOpenScene()
    {
        int injected = 0;
        foreach (var btn in Object.FindObjectsByType<Button>(FindObjectsSortMode.InstanceID))
        {
            if (!btn.TryGetComponent<ButtonHighlight>(out _))
            {
                Undo.AddComponent<ButtonHighlight>(btn.gameObject);
                injected++;
            }
        }
        Debug.Log($"[Highlight] Added to {injected} buttons in open scene.");
    }

    // -------------- every prefab asset ----------
    [MenuItem("Tools/UI/Add Highlight ► All Prefabs (Project)")]
    static void AddToAllPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int injected = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var root    = PrefabUtility.LoadPrefabContents(path);
            bool dirty  = false;

            foreach (var btn in root.GetComponentsInChildren<Button>(true))
            {
                if (!btn.TryGetComponent<ButtonHighlight>(out _))
                {
                    btn.gameObject.AddComponent<ButtonHighlight>();
                    dirty = true;
                    injected++;
                }
            }

            if (dirty) PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
        Debug.Log($"[Highlight] Added to {injected} buttons across all prefabs.");
    }
}
#endif
