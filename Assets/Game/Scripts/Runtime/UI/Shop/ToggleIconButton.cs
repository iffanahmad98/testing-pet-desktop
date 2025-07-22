using UnityEngine;
using UnityEngine.UI;

public class ToggleIconButton : MonoBehaviour
{
    public Image iconImage;
    public Sprite spriteOn;
    public Sprite spriteOff;

    private bool isOn = false;

    public System.Action<bool> OnToggleChanged; // optional callback

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(Toggle);
        UpdateVisual();
    }

    private void Toggle()
    {
        isOn = !isOn;
        UpdateVisual();
        OnToggleChanged?.Invoke(isOn);
    }

    private void UpdateVisual()
    {
        iconImage.sprite = isOn ? spriteOn : spriteOff;
        iconImage.color = isOn ? Color.white : new Color(1f, 1f, 1f, 0.5f); // Optional fade
    }

    public void SetState(bool on)
    {
        isOn = on;
        UpdateVisual();
    }

    public bool GetState() => isOn;
}
