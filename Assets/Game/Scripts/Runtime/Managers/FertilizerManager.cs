using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using MagicalGarden.Inventory;
using MagicalGarden.Farm;
namespace MagicalGarden.Manager
{
    public class FertilizerManager : MonoBehaviour
    {
        public static FertilizerManager Instance;

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
            if (activeTasks==null)
            {
                return;
            }
            if (activeTasks.IsComplete())
            {
                CompleteTask(activeTasks);
                activeTasks = null;
            }
        }

        public void StartCrafting(FertilizerRecipe recipe)
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
            }
        }

        private void CompleteTask(FertilizerTask task)
        {
            InventoryManager.Instance.AddItem(task.recipe.outputItem.item, task.recipe.outputItem.quantity);
            InventoryManager.Instance.inventoryUI.RefreshUI();
            // UIManager.Instance?.ShowPopup($"Selesai membuat: {task.recipe.outputItem.item.displayName}");

            // Opsional: efek visual atau suara
            // AudioManager.Instance?.Play("CraftComplete");
            // Instantiate(particles, machineTransform.position, Quaternion.identity);
        }
    }
    [Serializable]
    public class FertilizerRecipe
    {
        public string nameRecipe;
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
}