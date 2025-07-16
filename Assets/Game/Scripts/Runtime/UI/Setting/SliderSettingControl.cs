using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SliderSettingControl : MonoBehaviour
{
    public string settingName;
    public Slider slider;
    public Button resetButton;
    public TMP_InputField inputField;
    
    private float defaultValue;
    private float minValue;
    private float maxValue;
    private float currentValue;
    
    public Action<float> onValueChanged;
    
    public void Initialize(float defaultVal, float min, float max)
    {
        defaultValue = defaultVal;
        minValue = min;
        maxValue = max;
        
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        
        SetValue(defaultVal);
        
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        if (resetButton != null)
            resetButton.onClick.AddListener(() => SetValue(defaultValue));
        if (inputField != null)
            inputField.onEndEdit.AddListener(OnInputChanged);
    }
    
    private void OnSliderValueChanged(float newValue)
    {
        SetValue(newValue);
    }
    
    private void OnInputChanged(string value)
    {
        if (float.TryParse(value, out float result))
        {
            // If GamePosY, adjust the input value back
            if (settingName == "GamePosY")
                result -= 500;
                
            SetValue(result);
        }
        else
        {
            // Restore the current value if parsing failed
            UpdateDisplayText();
        }
    }
    
    private void SetValue(float newValue)
    {
        currentValue = Mathf.Clamp(newValue, minValue, maxValue);
        slider.value = currentValue;
        UpdateDisplayText();
        onValueChanged?.Invoke(currentValue);
    }
    
    private void UpdateDisplayText()
    {
        string displayValue = settingName == "GamePosY" ? 
            (currentValue + 500).ToString("F0") : 
            currentValue.ToString("F0");
            
        if (inputField != null)
            inputField.text = displayValue;
    }
    
    public float GetValue() => currentValue;
    
    public void SetValueWithoutNotify(float newValue)
    {
        currentValue = Mathf.Clamp(newValue, minValue, maxValue);
        slider.SetValueWithoutNotify(currentValue);
        UpdateDisplayText();
        // Don't invoke onValueChanged
    }
}
