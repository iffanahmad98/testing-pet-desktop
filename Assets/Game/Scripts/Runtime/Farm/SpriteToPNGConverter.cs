using UnityEngine;
using System.IO;

public class SpriteToPNGConverter : MonoBehaviour
{
    public Sprite spriteToSave; // Assign dari inspector

    [ContextMenu("Save Sprite to PNG")]
    public void SaveSpriteToPNG()
    {
        if (spriteToSave == null)
        {
            Debug.LogError("❌ Sprite tidak di-assign!");
            return;
        }

        // Ambil rect dan texture
        Texture2D originalTexture = spriteToSave.texture;
        Rect rect = spriteToSave.textureRect;

        // Buat texture baru dari sprite area
        Texture2D croppedTexture = new Texture2D((int)rect.width, (int)rect.height);
        Color[] pixels = originalTexture.GetPixels(
            (int)rect.x, (int)rect.y,
            (int)rect.width, (int)rect.height);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        // Convert ke PNG
        byte[] pngBytes = croppedTexture.EncodeToPNG();

        // Simpan ke disk
        string path = Application.dataPath + "/SavedSprite.png";
        File.WriteAllBytes(path, pngBytes);

        Debug.Log($"✅ Sprite saved to: {path}");
    }
}
