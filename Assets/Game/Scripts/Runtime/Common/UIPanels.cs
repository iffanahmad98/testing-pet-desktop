using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class UIPanels
{
    [Header("Float Menu Panel")] public GameObject UIFloatMenuPanel;
    public CanvasGroup UIFloatMenuCanvasGroup;

    [Header("Setting Panel")] public GameObject SettingPanel;
    public CanvasGroup SettingCanvasGroup;

    [Header("Shop Panel")] public GameObject ShopPanel;
    public CanvasGroup ShopCanvasGroup;

    [Header("Catalogue Panel")] public GameObject CataloguePanel;
    public CanvasGroup CatalogueCanvasGroup;

    [Header("Inventory Panel")] public GameObject InventoryPanel;
    public CanvasGroup InventoryCanvasGroup;
}

