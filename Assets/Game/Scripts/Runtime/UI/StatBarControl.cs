using UnityEngine;
using UnityEngine.UI;
using System;

public class StatBarControl : MonoBehaviour
{
    public Slider statSlider;
    private float maxValue = 100f;
    private float currentValue = 100f;
    
    public Action<float> onValueChanged;
    
    public void Initialize(float initialValue, float maxVal)
    {
        maxValue = maxVal;
            
        if (statSlider != null)
        {
            statSlider.minValue = 0;
            statSlider.maxValue = maxValue;
            statSlider.interactable = false; // Stats shouldn't be interactive
        }
        
        SetValue(initialValue);
    }
    
    public void SetValue(float newValue)
    {
        currentValue = Mathf.Clamp(newValue, 0, maxValue);
        
        if (statSlider != null)
            statSlider.value = currentValue;
            
        onValueChanged?.Invoke(currentValue);
    }
}
