using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using MagicalGarden.Inventory;
using MagicalGarden.Farm;
using TMPro;
using MagicalGarden.AI;
namespace MagicalGarden.Manager
{
    public class FertilizerManager : MonoBehaviour
    {
        public static FertilizerManager Instance;
        public MagicalGarden.Farm.UI.FertilizerUI fertilizerUI;
        public Animator craftingAnimator;
        public NPCFertilizer npc;

        [Header("All Fertilizer UIs")]
        public List<FertilizerUI> fertilizerUIs;
        // private List<FertilizerTask> activeTasks = new();
        private FertilizerTask activeTasks = null;

        [Header ("Data")]
        PlayerConfig playerConfig;
        FertilizerMachineData fertilizerMachineData;
        
        private void Awake()
        {
            Instance = this;
        }

        void Start ()
        {
            LoadFertilizerMachineData ();
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

        /*
        public void StartCrafting(FertilizerRecipe recipe, FertilizerType type)
        { // (NotUsed)
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
                StartCoroutine(npc.NPCFertiMake());
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
        */
        public void StartCrafting(FertilizerRecipe recipe, FertilizerType type, bool isLoad = false, DateTime dateTimeLoad = new DateTime ())
        { 
            if (!isLoad) {
                if (recipe.IsEligible ())
                {
                    recipe.UsingAllItems ();

                    var task = new FertilizerTask
                    {
                        recipe = recipe,
                        // startTime = DateTime.Now,
                        startTime = TimeManager.Instance.currentTime,
                        duration = recipe.craftDuration
                        
                    };

                    activeTasks = task;
                    StartCoroutine(npc.NPCFertiMake());
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

                    SaveFertilizerMachineData (type);
                }
            } else {
                var task = new FertilizerTask
                    {
                        recipe = recipe,
                        startTime = dateTimeLoad,
                        duration = recipe.craftDuration
                    };
                    
                    activeTasks = task;
                    StartCoroutine(npc.NPCFertiMake());
                    if (craftingAnimator != null)
                        craftingAnimator.SetBool("run", true);

                    
                    Debug.Log ("Refresh Task 2");
                    foreach (var ui in fertilizerUIs)
                    {
                        ui.btnCreate.gameObject.SetActive(false);
                        ui.btnDisable.gameObject.SetActive(true);
                    }
                    
                    foreach (var ui in fertilizerUIs)
                    {
                        bool isActive = ui.type == type;
                        ui.timeRemainingGO.SetActive(isActive);
                        ui.btnDone.gameObject.SetActive(!isActive);
                        ui.btnCreate.gameObject.SetActive(!isActive);
                        ui.btnDisable.gameObject.SetActive(!isActive);
                    }

                    SaveFertilizerMachineData (type);

                   
                
            }
        }

        private void CompleteTask(FertilizerTask task)
        {
            // (Ini coba hilangkan) InventoryManager.Instance.AddItem(task.recipe.outputItem.item, task.recipe.outputItem.quantity);
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

        private void ClaimFertilizerReward(FertilizerType type, FertilizerRecipe fertilizerRecipe)
        {
          //  (Not Used) InventoryManager.Instance.AddItem(activeTasks.recipe.outputItem.item, activeTasks.recipe.outputItem.quantity);
         //   InventoryManager.Instance.RefreshAllInventoryUI();
     
            if (playerConfig == null) {
                playerConfig = SaveSystem.PlayerConfig;
            }
           
            playerConfig.AddItemFarm (fertilizerRecipe.outputItem.item.itemId, fertilizerRecipe.outputItem.quantity);
            SaveSystem.SaveAll ();
            
            var ui = fertilizerUIs.Find(f => f.type == type);
            if (ui != null)
            {
                ui.btnDone.gameObject.SetActive(false);
                RefreshAllFertilizerUI();
            }

            activeTasks = null;

             SaveFertilizerMachineDataDone (type);
             SaveSystem.SaveAll ();
        }

        public void OnClickDoneGarden()
        {
            ClaimFertilizerReward(FertilizerType.Garden, fertilizerUI.allRecipes[0]);
        }

        public void OnClickDoneMana()
        {
            ClaimFertilizerReward(FertilizerType.Mana, fertilizerUI.allRecipes[1]);
        }

        public void OnClickDoneMoon()
        {
            ClaimFertilizerReward(FertilizerType.Moon, fertilizerUI.allRecipes[2]);
        }

        public void OnClickDoneSpirit()
        {
            ClaimFertilizerReward(FertilizerType.Spirit, fertilizerUI.allRecipes[3]);
        }

        public void RefreshAllFertilizerUI()
        {
            var items = InventoryManager.Instance.items;
            List <FertilizerRecipe> listFertilizerRecipe = fertilizerUI.GetAllRecipes ();
            int countNormal = items
                .Where(i => i.itemData.itemId == "poop_normal")
                .Sum(i => i.quantity);

            int countRare = items
                .Where(i => i.itemData.itemId == "poop_rare")
                .Sum(i => i.quantity);

            /* Not Used
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
            */
            for (int i=0; i < fertilizerUIs.Count; i++) {
                var ui = fertilizerUIs[i];
                ui.recipe = listFertilizerRecipe[i];
            }

            Debug.Log ("Refresh Task 1");
            foreach (var ui in fertilizerUIs)
            {
                bool canCraft = ui.recipe.IsEligible ();
                ui.btnCreate.gameObject.SetActive(canCraft);
                ui.btnDisable.gameObject.SetActive(!canCraft);
            }
            
        }

        #region Data
        void SaveFertilizerMachineData (FertilizerType type) {
            playerConfig.AddFertilizerMachineData (type, TimeManager.Instance.currentTime);

            SaveSystem.SaveAll ();
        }

        void SaveFertilizerMachineDataDone (FertilizerType type) {
            playerConfig.RemoveFertilizerMachineData (type);
        }

        public void LoadFertilizerMachineData () {
            if (playerConfig == null) {
                playerConfig = SaveSystem.PlayerConfig;
            }

            if (playerConfig.fertilizerMachineDatas.Count > 0) {
                fertilizerMachineData = playerConfig.fertilizerMachineDatas[0];
                FertilizerRecipe recipe = fertilizerUI.GetRecipe (fertilizerMachineData.fertilizerType);
               // StartCrafting (recipe,fertilizerMachineData.fertilizerType, true, fertilizerMachineData.startDate);
               
               DateTime fixedTime = TimeManager.Instance.GetFixedTimeInFuture (fertilizerMachineData.startDate);
               Debug.Log ("Your : " + fixedTime);
               fertilizerUI.OnStartButtonPressed (fertilizerMachineData.fertilizerType, recipe, true, fixedTime);
            } else {
                RefreshAllFertilizerUI();
            }
        }

        #endregion
    }
    [Serializable]
    public class FertilizerRecipe
    {
        public string nameRecipe;
        public FertilizerType type;
        public List<ItemStack> ingredients; // (Not Used)
        public List <EligibilityRuleSO> listEligibilityRuleSO = new List <EligibilityRuleSO> ();
        public ItemStack outputItem;

        [Tooltip("Durasi crafting dalam menit.")]
        public float durationInMinutes;
        public TimeSpan craftDuration => TimeSpan.FromMinutes(durationInMinutes);
        public string FormattedDuration => craftDuration.ToString(@"hh\:mm\:ss");

        public bool IsEligible () {
            foreach (EligibilityRuleSO itemEligible in listEligibilityRuleSO) {
                if (!itemEligible.IsEligible ()) {
                    return false;
                }
            }  
            return true;
        }

        public void UsingAllItems () {
            foreach (EligibilityRuleSO itemEligible in listEligibilityRuleSO) {
                EligibleMaterials eligibleMaterials = itemEligible as EligibleMaterials;
                eligibleMaterials.UsingAllItems ();
            }
        }
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
            return TimeManager.Instance.currentTime >= startTime + duration;
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
        public FertilizerRecipe recipe;
        public FertilizerType type;
        public GameObject timeRemainingGO;
        public TextMeshProUGUI timeRemainingText;
        public Button btnCreate;
        public Button btnDisable;
        public Button btnDone;


    }
    public enum FertilizerType { Garden, Mana, Moon, Spirit }
}