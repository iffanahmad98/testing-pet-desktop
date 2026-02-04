using System.Collections;
using UnityEngine;

public partial class UIManager
{
    #region Float Menu System

    public void FloatMenu()
    {
        if (_isAnimating) return;

        _onFloatMenuOpened = !_onFloatMenuOpened;

        if (_onFloatMenuOpened)
            StartCoroutine(FloatMenuAnim());
        else
            StartCoroutine(GroundMenuAnim());
    }

    public void GroundMenu()
    {
        if (_isAnimating || !_onFloatMenuOpened) return;

        _onFloatMenuOpened = false;
        StartCoroutine(GroundMenuAnim());
    }

    private IEnumerator FloatMenuAnim()
    {
        _isAnimating = true;

        panels.UIFloatMenuPanel.SetActive(true);

        Vector3 newPanelStartPos = _newMenuInitialPosition;
        newPanelStartPos.y -= _newMenuPanelRect.rect.height;
        _newMenuPanelRect.anchoredPosition = newPanelStartPos;
        panels.UIFloatMenuCanvasGroup.alpha = 0f;

        Vector3 buttonTargetPos = _buttonInitialPosition;
        buttonTargetPos.y += buttonSlideDistance;

        float duration = animationDuration;
        float elapsed = 0f;

        MonsterManager.instance.audio.PlaySFX("menu_open");

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easeOut = easeOutCurve.Evaluate(t);

            _buttonCanvasGroup.alpha = 1f - t;
            _buttonRect.anchoredPosition = Vector3.Lerp(_buttonInitialPosition, buttonTargetPos, easeOut);

            _newMenuPanelRect.anchoredPosition = Vector3.Lerp(newPanelStartPos, _newMenuInitialPosition, easeOut);
            panels.UIFloatMenuCanvasGroup.alpha = t;

            elapsed += Time.deltaTime;
            yield return null;
        }

        _buttonCanvasGroup.alpha = 0f;
        _buttonRect.anchoredPosition = buttonTargetPos;
        _newMenuPanelRect.anchoredPosition = _newMenuInitialPosition;
        panels.UIFloatMenuCanvasGroup.alpha = 1f;

        buttons.UIMenuButton.interactable = false;
        buttons.miniInventoryButton.interactable = false;
        _isAnimating = false;
    }

    private IEnumerator GroundMenuAnim()
    {
        _isAnimating = true;

        Vector3 newPanelTargetPos = _newMenuInitialPosition;
        newPanelTargetPos.y -= _newMenuPanelRect.rect.height;

        Vector3 buttonCurrentPos = _buttonInitialPosition;
        buttonCurrentPos.y += buttonSlideDistance;

        float duration = animationDuration;
        float elapsed = 0f;

        buttons.UIMenuButton.interactable = true;

        MonsterManager.instance.audio.PlaySFX("menu_close");

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easeIn = easeInCurve.Evaluate(t);

            _newMenuPanelRect.anchoredPosition = Vector3.Lerp(_newMenuInitialPosition, newPanelTargetPos, easeIn);
            panels.UIFloatMenuCanvasGroup.alpha = 1f - t;

            _buttonRect.anchoredPosition = Vector3.Lerp(buttonCurrentPos, _buttonInitialPosition, easeIn);
            _buttonCanvasGroup.alpha = t;

            elapsed += Time.deltaTime;
            yield return null;
        }

        _newMenuPanelRect.anchoredPosition = newPanelTargetPos;
        panels.UIFloatMenuCanvasGroup.alpha = 0f;
        _buttonRect.anchoredPosition = _buttonInitialPosition;
        _buttonCanvasGroup.alpha = 1f;

        panels.UIFloatMenuPanel.SetActive(false);
        buttons.miniInventoryButton.interactable = true;
        _isAnimating = false;
    }

    #endregion
}
