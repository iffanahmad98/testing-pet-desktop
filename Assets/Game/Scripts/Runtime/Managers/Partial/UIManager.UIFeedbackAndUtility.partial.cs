using Coffee.UIExtensions;
using UnityEngine;

public partial class UIManager
{
    #region UI Feedback

    public void ShowMessage(string message, float duration = 1f)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideMessage));
        Invoke(nameof(HideMessage), duration);
    }

    private void HideMessage()
    {
        messageText.gameObject.SetActive(false);
    }

    #endregion

    #region UI Vfx

    public void InitUnlockedMenuVfx(RectTransform pos)
    {
        GameObject obj = Instantiate(unlockedBtnVfxPrefab);
        obj.GetComponent<RectTransform>().position = pos.position;
        obj.GetComponent<RectTransform>().SetParent(vfxParent.transform);
        obj.GetComponent<UIParticle>().Play();
        //obj.GetComponent<ParticleSystem>().Emit(1);
    }

    #endregion

    #region Monster System

    private void UpdatePoopCounterValue(int newPoopAmount)
    {
    }

    #endregion

    #region Utility

    private void MinimizeApplication()
    {
#if UNITY_EDITOR
        // Stop play mode if running in Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
        return;
#endif
        // Minimize the application in standalone builds
        var transparentWindow = ServiceLocator.Get<TransparentWindow>();
        if (transparentWindow != null)
        {
            transparentWindow.MinimizeWindow();
            GroundMenu();
        }
        else
        {
            Debug.LogWarning("TransparentWindow service not found - cannot minimize");

#if !UNITY_STANDALONE_WIN && !UNITY_EDITOR_WIN
            Application.Quit();
#endif
        }
    }

    #endregion
}
