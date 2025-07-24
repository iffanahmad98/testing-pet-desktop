using UnityEngine;
using System.Collections.Generic;
using MagicalGarden.Manager;
using MagicalGarden.Shop;

namespace MagicalGarden.Inventory
{
    public class ShopUI : MonoBehaviour
    {
        public GameObject dropFlyIcon;
        public Transform itemContainer;
        public GridContentResizer gridContentResizer;

        private void OnEnable()
        {
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (ShopManager.Instance == null) return;
            gridContentResizer.Refresh(itemContainer.childCount);
        }
    }
}