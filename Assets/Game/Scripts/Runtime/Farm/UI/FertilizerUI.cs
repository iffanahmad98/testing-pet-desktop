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
        public TextMeshProUGUI gardenRemaining;
        public TextMeshProUGUI manaRemaining;
        public TextMeshProUGUI moonlightRemaining;
        public TextMeshProUGUI spiritRemaining;
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
        //    recipeDescriptionText.text = desc;
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

        // public void OnStartButtonPressed()
        // {
        //     int index = recipeDropdown.value;
        //     var selectedRecipe = allRecipes[index];
        //     if (FertilizerManager.Instance.IsHasActiveTask())
        //     {
        //         return;
        //     }

        //     if (InventoryManager.Instance.HasItems(selectedRecipe.ingredients))
        //     {
        //         FertilizerManager.Instance.StartCrafting(selectedRecipe, selectedRecipe.type);
        //         StartProgressUI(selectedRecipe);
        //         Debug.Log("Mulai membuat pupuk!");
        //     }
        //     else
        //     {
        //         progressText.text = "Bahan tidak mencukupi!";
        //     }
        // }
        public void OnStartBtnGarden() => OnStartButtonPressed(FertilizerType.Garden);
        public void OnStartBtnMana() => OnStartButtonPressed(FertilizerType.Mana);
        public void OnStartBtnMoon() => OnStartButtonPressed(FertilizerType.Moon);
        public void OnStartBtnSpirit() => OnStartButtonPressed(FertilizerType.Spirit);
        public void OnStartButtonPressed(FertilizerType type)
        {
            var recipeList = allRecipes.Where(r => r.type == type).ToList();
            int index = recipeDropdown.value;
            if (index >= recipeList.Count) return;

           var selectedRecipe = recipeList[index];
            if (FertilizerManager.Instance.IsHasActiveTask())
                return;

            
            if (InventoryManager.Instance.HasItems(selectedRecipe.ingredients))
            {
                FertilizerManager.Instance.StartCrafting(selectedRecipe, type);
                StartProgressUI(selectedRecipe);
            }
            else
            {
                Debug.LogError("bahan tidak cukup");
            }
            
            switch (type)
            {
                case FertilizerType.Garden:
                    progressText = gardenRemaining;
                    break;
                case FertilizerType.Mana:
                    progressText = manaRemaining;
                    break;
                case FertilizerType.Moon:
                    progressText = moonlightRemaining;
                    break;
                case FertilizerType.Spirit:
                    progressText = spiritRemaining;
                    break;
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

            progressText.text = $"{remaining:hh\\:mm\\:ss}";

            if (progress >= 1f)
            {
                activeTask = null;
            }
        }
    }
}
