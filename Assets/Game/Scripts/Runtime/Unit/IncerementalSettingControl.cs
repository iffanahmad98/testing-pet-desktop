using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class IncrementSettingControl : MonoBehaviour
{
    public Button minusTenButton;
    public Button minusOneButton;
    public Button plusOneButton;
    public Button plusTenButton;
    public Button resetButton;
    public TMP_InputField inputField;

    private float defaultValue;
    private float minValue;
    private float maxValue;

    public Action<float> onValueChanged;

    private float currentValue;

    public void Initialize(float defaultVal, float min, float max)
    {
        defaultValue = defaultVal;
        minValue = min;
        maxValue = max;

        SetValue(defaultVal);

        minusTenButton.onClick.AddListener(() => ChangeValue(-10));
        minusOneButton.onClick.AddListener(() => ChangeValue(-1));
        plusOneButton.onClick.AddListener(() => ChangeValue(1));
        plusTenButton.onClick.AddListener(() => ChangeValue(10));
        resetButton.onClick.AddListener(() => SetValue(defaultVal));
        inputField.onEndEdit.AddListener(OnInputChanged);
    }

    private void ChangeValue(float delta)
    {
        SetValue(currentValue + delta);
    }

    private void SetValue(float newValue)
    {
        currentValue = Mathf.Clamp(newValue, minValue, maxValue);
        inputField.text = currentValue.ToString("F0");
        onValueChanged?.Invoke(currentValue);
    }

    private void OnInputChanged(string value)
    {
        if (float.TryParse(value, out float result))
        {
            SetValue(result);
        }
        else
        {
            inputField.text = currentValue.ToString("F0");
        }
    }

    public float GetValue() => currentValue;
    public void SetValueWithoutNotify(float newValue)
    {
        currentValue = Mathf.Clamp(newValue, minValue, maxValue);
        inputField.text = currentValue.ToString("F0");

        // Don’t invoke onValueChanged
    }

}
