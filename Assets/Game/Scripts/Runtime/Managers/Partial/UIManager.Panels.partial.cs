using DG.Tweening;
using UnityEngine;

public partial class UIManager
{
    #region Panel Management

    private void HideAllPanels()
    {
        panels.UIFloatMenuPanel.SetActive(false);

        // // Hide Setting Panel
        // SettingCanvasGroup.alpha = 0f;
        // SettingCanvasGroup.interactable = false;
        // SettingCanvasGroup.blocksRaycasts = false;

        // // Hide Shop Panel
        // ShopCanvasGroup.alpha = 0f;
        // ShopCanvasGroup.interactable = false;
        // ShopCanvasGroup.blocksRaycasts = false;

        // // Hide Inventory Panel
        // InventoryCanvasGroup.alpha = 0f;
        // InventoryCanvasGroup.interactable = false;
        // InventoryCanvasGroup.blocksRaycasts = false;

        // // Hide Catalogue Panel
        // CatalogueCanvasGroup.alpha = 0f;
        // CatalogueCanvasGroup.interactable = false;
        // CatalogueCanvasGroup.blocksRaycasts = false;
    }

    public void FadePanel(GameObject panel, CanvasGroup canvasGroup, bool fadeIn, float duration = 0.3f,
        float scalePop = 1.08f, float scaleDuration = 0.15f, bool isActive = false)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        if (fadeIn)
        {
            panel.SetActive(true);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0f;
            rect.localScale = Vector3.one;
            panel.transform.SetAsLastSibling();

            MonsterManager.instance.audio.PlaySFX("button_click");

            canvasGroup.DOFade(1f, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                });
        }
        else
        {
            MonsterManager.instance.audio.PlaySFX("menu_close");
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0f, duration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => panel.SetActive(isActive));
        }
    }

    #endregion
}
