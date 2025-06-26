using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BiomeLayerSystem
{
    public GameObject layerObject; // The GameObject representing the layer
    public string layerName; // Name of the layer for identification
    public bool isActive = true; // Whether the layer is currently active

    // Method to toggle the active state of the layer
    public void ToggleLayer()
    {
        isActive = !isActive;
        if (layerObject != null)
        {
            layerObject.SetActive(isActive);
        }
    }

    // Method to initialize the layer state
    public void InitializeLayer()
    {
        if (layerObject != null)
        {
            layerObject.SetActive(isActive);
        }
    }
}
