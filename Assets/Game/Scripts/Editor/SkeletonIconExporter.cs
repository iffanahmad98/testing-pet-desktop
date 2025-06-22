/*  Assets ► Spine ► Export Skeleton Icons
 *  – Exports a PNG that looks exactly like the thumbnail Unity shows in the Project view.
 *  – Puts the PNG next to the .asset file with the same name.                */
using UnityEditor;
using UnityEngine;
using System.IO;
using Spine.Unity;

public static class SkeletonIconExporter
{
    [MenuItem("Assets/Spine/Export Skeleton Icon", true)]
    static bool Validate() => Selection.activeObject is SkeletonDataAsset;

    [MenuItem("Assets/Spine/Export Skeleton Icon")]
    static void ExportIcon()
    {
        var asset = (SkeletonDataAsset)Selection.activeObject;
        var path  = AssetDatabase.GetAssetPath(asset);

        // Ask Unity to start (or finish) rendering the thumbnail
        Texture2D preview = AssetPreview.GetAssetPreview(asset);
        if (preview == null)                  // still rendering – wait one frame
        {
            EditorApplication.delayCall += ExportIcon;   // will re-enter with same selection
            return;
        }

        // Encode to PNG
        byte[] pngBytes = preview.EncodeToPNG();
        string outPath  = Path.ChangeExtension(path, "png");
        File.WriteAllBytes(outPath, pngBytes);
        AssetDatabase.Refresh();

        Debug.Log($"Icon saved → {outPath}");
    }
}
