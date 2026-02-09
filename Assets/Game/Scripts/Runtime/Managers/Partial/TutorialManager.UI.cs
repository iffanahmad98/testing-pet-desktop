using UnityEngine;
using DG.Tweening;
public partial class TutorialManager
{
    public void ShowSkipButtonAnimated()
    {
        if (skipTutorialButton == null)
            return;

        var rect = skipTutorialButton.transform as RectTransform;
        var cg = skipTutorialButton.GetComponent<CanvasGroup>();
        if (rect == null || cg == null)
            return;

        rect.DOKill();
        cg.DOKill();

        var targetPos = rect.anchoredPosition;
        var startPos = targetPos;
        startPos.y -= 80f;
        rect.anchoredPosition = startPos;

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        rect.DOAnchorPos(targetPos, 0.3f).SetEase(Ease.OutQuad);
        cg.DOFade(1f, 0.3f).OnComplete(() =>
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        });
    }

    public void HideSkipButtonAnimated()
    {
        if (skipTutorialButton == null)
            return;

        var rect = skipTutorialButton.transform as RectTransform;
        var cg = skipTutorialButton.GetComponent<CanvasGroup>();
        if (rect == null || cg == null)
            return;

        rect.DOKill();
        cg.DOKill();

        var startPos = rect.anchoredPosition;
        var targetPos = startPos;
        targetPos.y -= 80f;

        cg.interactable = false;
        cg.blocksRaycasts = false;

        rect.DOAnchorPos(targetPos, 0.25f).SetEase(Ease.InQuad);
        cg.DOFade(0f, 0.25f);
    }
}