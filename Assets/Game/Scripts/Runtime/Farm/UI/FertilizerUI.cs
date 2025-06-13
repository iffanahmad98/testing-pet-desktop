using UnityEngine;
using System.Collections.Generic;
using MagicalGarden.Manager;
using MagicalGarden.Inventory;
using System.Linq;
using TMPro;
using System;

namespace MagicalGarden.Farm.UI
{
    public class FertilizerUI : MonoBehaviour
    {
        public TMP_Dropdown recipeDropdown;
        public TextMeshProUGUI recipeDescriptionText;
        public TextMeshProUGUI progressText;

        public List<FertilizerRecipe> allRecipes;
        private FertilizerTask activeTask;

        private void Start()
        {
            recipeDropdown.onValueChanged.AddListener(OnRecipeSelected);
            PopulateDropdown();
        }

        private void PopulateDropdown()
        {
            recipeDropdown.ClearOptions();
            var options = allRecipes.Select(r => r.outputItem.item.displayName).ToList();
            recipeDropdown.AddOptions(options);
            OnRecipeSelected(0);
        }

        private void OnRecipeSelected(int index)
        {
            var recipe = allRecipes[index];
            string desc = GetRecipeDescription(recipe);
            recipeDescriptionText.text = desc;
        }

        private string GetRecipeDescription(FertilizerRecipe recipe)
        {
            string time = $"{recipe.craftDuration.TotalHours:0.#} jam";
            string ingredients = string.Join("\n", recipe.ingredients.Select(i =>
                $"- {i.item.displayName} x{i.quantity}"
            ));
            string result = $"Hasil: {recipe.outputItem.item.displayName} x{recipe.outputItem.quantity}";

            return $"{result}\nWaktu: {time}\nBahan:\n{ingredients}";
        }

        public void OnStartButtonPressed()
        {
            int index = recipeDropdown.value;
            var selectedRecipe = allRecipes[index];

            if (InventoryManager.Instance.HasItems(selectedRecipe.ingredients))
            {
                FertilizerManager.Instance.StartCrafting(selectedRecipe);
                StartProgressUI(selectedRecipe);
                Debug.Log("Mulai membuat pupuk!");
            }
            else
            {
                progressText.text = "Bahan tidak mencukupi!";
            }
        }
        private void StartProgressUI(FertilizerRecipe recipe)
        {
            activeTask = new FertilizerTask
            {
                recipe = recipe,
                startTime = DateTime.Now,
                duration = recipe.craftDuration
            };
        }

        private void Update()
        {
            if (activeTask == null) return;

            TimeSpan elapsed = DateTime.Now - activeTask.startTime;
            float progress = Mathf.Clamp01((float)(elapsed.TotalSeconds / activeTask.duration.TotalSeconds));
            TimeSpan remaining = activeTask.duration - elapsed;
            if (remaining.TotalSeconds < 0) remaining = TimeSpan.Zero;

            progressText.text = $"{activeTask.recipe.outputItem.item.displayName}\n" +
                                $"Sisa: {remaining:hh\\:mm\\:ss} ({progress * 100:0}%)";

            if (progress >= 1f)
            {
                activeTask = null;
            }
        }
    }
}
