using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using MagicalGarden.Inventory;
using MagicalGarden.Farm;
using TMPro;
namespace MagicalGarden.Manager
{
    public class FertilizerManager : MonoBehaviour
    {
        public static FertilizerManager Instance;
        public Animator craftingAnimator;

        [Header("All Fertilizer UIs")]
        public List<FertilizerUI> fertilizerUIs;
        // private List<FertilizerTask> activeTasks = new();
        private FertilizerTask activeTasks = null;

        private void Awake()
        {
            Instance = this;
        }

        public bool IsHasActiveTask()
        {
            return activeTasks != null;
        }

        private void Update()
        {
            // for (int i = activeTasks.Count - 1; i >= 0; i--)
            // {
            //     if (activeTasks[i].IsComplete())
            //     {
            //         CompleteTask(activeTasks[i]);
            //         activeTasks.RemoveAt(i);
            //     }
            // }
            if (activeTasks == null)
            {
                return;
            }
            if (activeTasks.IsComplete())
            {
                CompleteTask(activeTasks);
            }
        }

        public void StartCrafting(FertilizerRecipe recipe, FertilizerType type)
        {
            if (InventoryManager.Instance.HasItems(recipe.ingredients))
            {
                InventoryManager.Instance.RemoveItems(recipe.ingredients);

                var task = new FertilizerTask
                {
                    recipe = recipe,
                    startTime = DateTime.Now,
                    duration = recipe.craftDuration
                };

                activeTasks = task;
                if (craftingAnimator != null)
                    craftingAnimator.SetBool("run", true);
                foreach (var ui in fertilizerUIs)
                {
                    bool isActive = ui.type == type;
                    ui.timeRemainingGO.SetActive(isActive);
                    ui.btnDone.gameObject.SetActive(!isActive);
                    ui.btnCreate.gameObject.SetActive(!isActive);
                    ui.btnDisable.gameObject.SetActive(!isActive);
                }
            }
        }

        private void CompleteTask(FertilizerTask task)
        {
            // InventoryManager.Instance.AddItem(task.recipe.outputItem.item, task.recipe.outputItem.quantity);
            // InventoryManager.Instance.RefreshAllInventoryUI();
            foreach (var ui in fertilizerUIs)
            {
                bool isTarget = ui.type == task.recipe.type;

                ui.timeRemainingGO.SetActive(false);
                ui.btnCreate.gameObject.SetActive(!isTarget);  
                ui.btnDisable.gameObject.SetActive(!isTarget);
                ui.btnDone.gameObject.SetActive(isTarget);
            }

            if (craftingAnimator != null)
                craftingAnimator.SetBool("run", false);
            // UIManager.Instance?.ShowPopup($"Selesai membuat: {task.recipe.outputItem.item.displayName}");

            // Opsional: efek visual atau suara
            // AudioManager.Instance?.Play("CraftComplete");
            // Instantiate(particles, machineTransform.position, Quaternion.identity);
        }

        private void ClaimFertilizerReward(FertilizerType type)
        {
            InventoryManager.Instance.AddItem(activeTasks.recipe.outputItem.item, activeTasks.recipe.outputItem.quantity);
            InventoryManager.Instance.RefreshAllInventoryUI();
            var ui = fertilizerUIs.Find(f => f.type == type);
            if (ui != null)
            {
                ui.btnDone.gameObject.SetActive(false);
                RefreshAllFertilizerUI();
            }

            activeTasks = null;
        }

        public void OnClickDoneGarden()
        {
            ClaimFertilizerReward(FertilizerType.Garden);
        }

        public void OnClickDoneMana()
        {
            ClaimFertilizerReward(FertilizerType.Mana);
        }

        public void OnClickDoneMoon()
        {
            ClaimFertilizerReward(FertilizerType.Moon);
        }

        public void OnClickDoneSpirit()
        {
            ClaimFertilizerReward(FertilizerType.Spirit);
        }

        public void RefreshAllFertilizerUI()
        {
            var items = InventoryManager.Instance.items;

            int countNormal = items
                .Where(i => i.itemData.itemId == "poop_normal")
                .Sum(i => i.quantity);

            int countRare = items
                .Where(i => i.itemData.itemId == "poop_rare")
                .Sum(i => i.quantity);

            foreach (var ui in fertilizerUIs)
            {
                bool canCraft = false;

                switch (ui.type)
                {
                    case FertilizerType.Garden:
                        canCraft = countNormal >= 5;
                        break;
                    case FertilizerType.Mana:
                        canCraft = countRare >= 2;
                        break;
                    case FertilizerType.Moon:
                        canCraft = countRare >= 5;
                        break;
                    case FertilizerType.Spirit:
                        canCraft = countNormal >= 3 && countRare >= 7;
                        break;
                }

                // Toggle tombol aktif/nonaktif
                ui.btnCreate.gameObject.SetActive(canCraft);
                ui.btnDisable.gameObject.SetActive(!canCraft);
            }
        }
    }
    [Serializable]
    public class FertilizerRecipe
    {
        public string nameRecipe;
        public FertilizerType type;
        public List<ItemStack> ingredients;
        public ItemStack outputItem;

        [Tooltip("Durasi crafting dalam menit.")]
        public float durationInMinutes;
        public TimeSpan craftDuration => TimeSpan.FromMinutes(durationInMinutes);
        public string FormattedDuration => craftDuration.ToString(@"hh\:mm\:ss");
    }

    [Serializable]
    public class FertilizerTask
    {
        public FertilizerRecipe recipe;
        public FertilizerType type;
        public DateTime startTime;
        public TimeSpan duration;

        public bool IsComplete()
        {
            return DateTime.Now >= startTime + duration;
        }
    }

    [Serializable]
    public class ItemStack
    {
        public ItemData item;
        public int quantity;

        public ItemStack(ItemData item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }
    [System.Serializable]
    public class FertilizerUI
    {
        public FertilizerType type;
        public GameObject timeRemainingGO;
        public TextMeshProUGUI timeRemainingText;
        public Button btnCreate;
        public Button btnDisable;
        public Button btnDone;
    }
    public enum FertilizerType { Garden, Mana, Moon, Spirit }
}