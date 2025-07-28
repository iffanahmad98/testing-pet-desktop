using UnityEngine;
using UnityEditor;

public class ReplaceSelectedWithPrefab : EditorWindow
{
    private GameObject prefab;

    [MenuItem("Tools/Replace Selected With Prefab")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceSelectedWithPrefab>("Replace With Prefab");
    }

    void OnGUI()
    {
        GUILayout.Label("Replace Selected GameObjects", EditorStyles.boldLabel);

        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

        if (GUILayout.Button("Ganti Yang Dipilih"))
        {
            Replace();
        }
    }

    void Replace()
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefab belum dipilih.");
            return;
        }

        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Tidak ada GameObject yang dipilih.");
            return;
        }

        foreach (GameObject go in selectedObjects)
        {
            Transform parent = go.transform.parent;
            Vector3 pos = go.transform.position;
            Quaternion rot = go.transform.rotation;
            Vector3 scale = go.transform.localScale;

            Undo.DestroyObjectImmediate(go);
            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            newObj.transform.position = pos;
            newObj.transform.rotation = rot;
            newObj.transform.localScale = scale;

            Undo.RegisterCreatedObjectUndo(newObj, "Replace with Prefab");
        }

        Debug.Log($"{selectedObjects.Length} GameObject telah diganti.");
    }
}