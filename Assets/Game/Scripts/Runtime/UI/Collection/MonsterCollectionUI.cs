using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MonsterCollectionUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button closeButton;
    public Button catalogueButton;

    [Header("UI Components")]
    public CanvasGroup monsterCatalogueCanvasGroup;
    public CanvasGroup monsterCollectionCanvasGroup;

    private void Awake()
    {
        // Initialize listeners
        InitListeners();
        // Get the CanvasGroup component
        monsterCollectionCanvasGroup = GetComponent<CanvasGroup>();
    }

    private void InitListeners()
    {
        // Remove existing listeners to prevent duplicates
        closeButton.onClick.RemoveAllListeners();
        catalogueButton.onClick.RemoveAllListeners();
        // Add new listeners
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        catalogueButton.onClick.AddListener(OnCloseButtonClicked);
    }

    private void OnCatalogueButtonClicked()
    {

    }

    private void OnCloseButtonClicked()
    {
        monsterCollectionCanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
        {
            monsterCollectionCanvasGroup.interactable = false;
            monsterCollectionCanvasGroup.blocksRaycasts = false;
        });
    }
}
