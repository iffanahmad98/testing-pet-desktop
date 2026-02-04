using System.Collections;
using UnityEngine;

public partial class UIManager
{
    #region Mini Window System

    private void InitMiniWindow()
    {
        _windowButtonRect = buttons.miniWindowButton?.GetComponent<RectTransform>();
        _windowButtonCanvasGroup = buttons.miniWindowButton?.GetComponent<CanvasGroup>();

        if (_windowButtonCanvasGroup == null && buttons.miniWindowButton != null)
            _windowButtonCanvasGroup = buttons.miniWindowButton.gameObject.AddComponent<CanvasGroup>();

        if (_windowButtonRect != null)
        {
            _originalWindowButtonScale = _windowButtonRect.localScale;
            _originalWindowButtonPosition = _windowButtonRect.anchoredPosition;
        }

        if (gameContentParent != null)
        {
            _gameContentCanvasGroup = gameContentParent.GetComponent<CanvasGroup>();
            if (_gameContentCanvasGroup == null)
                _gameContentCanvasGroup = gameContentParent.AddComponent<CanvasGroup>();
        }

        if (buttons.miniWindowButton != null)
            buttons.miniWindowButton.gameObject.SetActive(false);
    }

    public void ToggleMiniWindowMode()
    {
        if (_isAnimating) return;

        _isMiniWindowMode = !_isMiniWindowMode;

        if (_isMiniWindowMode)
            EnterMiniWindowMode();
        else
            ExitMiniWindowMode();
    }

    private void EnterMiniWindowMode()
    {
        MonsterManager.instance.audio.PlaySFX("menu_close");
        if (_gameContentCanvasGroup != null)
        {
            _gameContentCanvasGroup.alpha = gameAreaOpacity;
            _gameContentCanvasGroup.interactable = false;
            _gameContentCanvasGroup.blocksRaycasts = false;
        }

        if (panels.UIFloatMenuPanel != null)
            panels.UIFloatMenuPanel.SetActive(false);

        if (buttons.miniWindowButton != null)
        {
            buttons.miniWindowButton.gameObject.SetActive(true);

            if (_windowButtonCanvasGroup != null)
            {
                _windowButtonCanvasGroup.alpha = 1f;
                _windowButtonCanvasGroup.interactable = true;
                _windowButtonCanvasGroup.blocksRaycasts = true;
            }

            if (_windowButtonRect != null)
                StartCoroutine(AnimateButtonScale(_originalWindowButtonScale,
                    _originalWindowButtonScale * miniWindowScale));
        }

        transparentWindow?.SetTopMostMode(false);
    }

    private void ExitMiniWindowMode()
    {
        MonsterManager.instance.audio.PlaySFX("menu_open");
        if (_gameContentCanvasGroup != null)
        {
            _gameContentCanvasGroup.alpha = 1f;
            _gameContentCanvasGroup.interactable = true;
            _gameContentCanvasGroup.blocksRaycasts = true;
        }

        if (buttons.miniWindowButton != null && _windowButtonRect != null)
            StartCoroutine(AnimateButtonScaleAndHide());
        else if (buttons.miniWindowButton != null)
            buttons.miniWindowButton.gameObject.SetActive(false);

        if (panels.UIFloatMenuPanel != null)
            panels.UIFloatMenuPanel.SetActive(true);

        if (_onFloatMenuOpened && panels.UIFloatMenuCanvasGroup != null)
        {
            panels.UIFloatMenuCanvasGroup.alpha = 1f;
            panels.UIFloatMenuCanvasGroup.interactable = true;
            panels.UIFloatMenuCanvasGroup.blocksRaycasts = true;
        }
        else if (panels.UIFloatMenuCanvasGroup != null)
        {
            panels.UIFloatMenuCanvasGroup.alpha = 0f;
            panels.UIFloatMenuCanvasGroup.interactable = false;
            panels.UIFloatMenuCanvasGroup.blocksRaycasts = false;
        }

        transparentWindow?.SetTopMostMode(true);
    }

    private IEnumerator AnimateButtonScale(Vector3 fromScale, Vector3 toScale)
    {
        float elapsed = 0f;

        while (elapsed < scaleAnimDuration)
        {
            float t = elapsed / scaleAnimDuration;
            float easedT = scaleAnimCurve.Evaluate(t);

            _windowButtonRect.localScale = Vector3.Lerp(fromScale, toScale, easedT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _windowButtonRect.localScale = toScale;
    }

    private IEnumerator AnimateButtonScaleAndHide()
    {
        Vector3 fromScale = _windowButtonRect.localScale;
        Vector3 toScale = _originalWindowButtonScale;

        float elapsed = 0f;

        while (elapsed < scaleAnimDuration)
        {
            float t = elapsed / scaleAnimDuration;
            float easedT = scaleAnimCurve.Evaluate(t);

            _windowButtonRect.localScale = Vector3.Lerp(fromScale, toScale, easedT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _windowButtonRect.localScale = toScale;
        _windowButtonRect.anchoredPosition = _originalWindowButtonPosition;
        buttons.miniWindowButton.gameObject.SetActive(false);
    }

    #endregion
}
