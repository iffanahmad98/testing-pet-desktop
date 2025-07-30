#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UI;

public static class ButtonHighlightPreset
{
    // Creates a preset that automatically attaches ButtonHighlight
    [MenuItem("Tools/UI/Create \"DefaultButton\" Preset")]
    static void CreatePreset()
    {
        // make a temporary button
        var go   = new GameObject("TempButton", typeof(RectTransform), typeof(Image), typeof(Button));
        go.AddComponent<ButtonHighlight>();

        // build a Preset that targets the Button component *type*
        var preset = new Preset(go.GetComponent<Button>());
        AssetDatabase.CreateAsset(preset, "Assets/UI/DefaultButton.preset");

        // mark it "Default"
        // Preset.GetDefaultTypesForObject(go.GetComponent<Button>());
        // Preset.SetDefaultPreset(preset, true);

        Object.DestroyImmediate(go);
        AssetDatabase.SaveAssets();
        Debug.Log("[Highlight] \"DefaultButton.preset\" created and set as default.");
    }
}
#endif
