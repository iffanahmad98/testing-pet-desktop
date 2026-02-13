using UnityEngine;
using UnityEngine.UI;

public class WindowAdjustableCanvas : MonoBehaviour
{
    [SerializeField] TransparentWindow transparentWindow;

    [Header("UI")]
    [SerializeField] Sprite checkboxOn, checkboxOff;
    [SerializeField] Toggle toggleWindowOnTop;

    bool isEnableWindowOnTop;

    void Start()
    {
        toggleWindowOnTop.onValueChanged.AddListener(EnableToggleWindow);
        DefaultStartSet();
    }

    void DefaultStartSet()
    {
        toggleWindowOnTop.isOn = false;
        EnableToggleWindow(false);
    }

    void EnableToggleWindow(bool isOn)
    {
        isEnableWindowOnTop = isOn;

        if (isOn)
        {
            toggleWindowOnTop.image.sprite = checkboxOn;
          //  Debug.LogError("Enable window");
            transparentWindow.ChangeEnableTopMost (true);
            // transparentWindow.SetWindowOnTop(true);
        }
        else
        {
            toggleWindowOnTop.image.sprite = checkboxOff;
         //   Debug.LogError("Disable window");
            transparentWindow.ChangeEnableTopMost (false);
            // transparentWindow.SetWindowOnTop(false);
        }
    }
}
