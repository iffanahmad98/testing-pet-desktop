using MagicalGarden.Manager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class RarityWeight
{
    public MonsterType type;
    [Range(0f, 100f)]
    public float weight;

    [Header("Soft Pity Configuration")]
    public bool hasSoftPity = false;
    [Tooltip("Number of pulls before soft pity starts for this rarity")]
    public int softPityStartAt = 70;
    [Tooltip("Percentage increase per pull after soft pity starts (e.g., 5 = 5%)")]
    [Range(0f, 50f)]
    public float softPityIncreasePercent = 5f;

    [Header("Hard Pity Configuration")]
    public bool hasHardPity = false;
    [Tooltip("Guaranteed this rarity at this pull count")]
    public int hardPityAt = 90;
    [Tooltip("Range for hard pity rate (Min-Max weight for this rarity)")]
    public Vector2 hardPityRateRange = new Vector2(80f, 100f);
}

[System.Serializable]
public class PityCounterDisplay
{
    public MonsterType rarityType;
    public int currentPullCount;
}

public class GachaManager : MonoBehaviour
{
    [Header("Gacha Configuration")]
    public MonsterDatabaseSO monsterDatabase;
    public List<RarityWeight> rarityWeights;
    public int gachaCost = 0;

    // Internal counters
    private int totalPullCount = 0;
    private Dictionary<MonsterType, int> pityCounters = new Dictionary<MonsterType, int>();
    private const string TOTAL_PULL_COUNT_KEY = "GachaTotalPullCount";
    private const string PITY_COUNTER_PREFIX = "GachaPityCounter_";

    [Header("Allowed Rarities")]
    [SerializeField]
    private List<MonsterType> allowedRarities = new List<MonsterType>
    {
        MonsterType.Rare,
        MonsterType.Mythic,
        MonsterType.Legend
    };

    [Header("Gacha Counter Info (Read Only)")]
    [SerializeField, Tooltip("Total number of gacha pulls")]
    private int currentTotalPulls = 0;
    [SerializeField, Tooltip("Pull count since last获得 of each rarity type")]
    private List<PityCounterDisplay> pityCounterDisplay = new List<PityCounterDisplay>();

    [Header("UI References")]
    public GachaResultPanel gachaResultPanel;
    public TextMeshProUGUI gachaPriceText;
    public Image buyImage;
    public Image coinImage;
    [SerializeField] Button drawGachaButton;
    [Header("Grayscale Components")]
    public Material grayscaleMaterial;
    [Header ("Audio")]
    public AudioClip openEggClip;
    [SerializeField] bool cdGacha = false;
    bool onceEvent = false;
    private void Awake()
    {
        ServiceLocator.Register(this);
        ValidateConfiguration();
        LoadPityCounters();
    }

    void Start()
    {
        if (currentTotalPulls >= 7 && TimeManager.Instance.CheckIfMonday() && SaveSystem.PlayerConfig.isMondayReset == true)
        {
            SaveSystem.PlayerConfig.isMondayReset = false;
        }
        else
        {
            Debug.Log($"Cannot Reset Because: CurrentTotalPulls = {currentTotalPulls}, TimeManager.CheckIfMonday = {TimeManager.Instance.CheckIfMonday()}, and SaveSystem.IsMondayReset = {SaveSystem.PlayerConfig.isMondayReset}");
        }

        if (SaveSystem.PlayerConfig.isMondayReset == false)
        {
            Debug.Log($"Pertama kali login");

            totalPullCount = 0;
            TimeManager.Instance.ResetGatchaWeek();
            SaveSystem.SetMondayReset();
        }

        UpdateGatchaCost();
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<GachaManager>();
    }

    private void ValidateConfiguration()
    {
        if (monsterDatabase == null)
        {
            return;
        }

        var invalidWeights = rarityWeights.Where(w => !allowedRarities.Contains(w.type)).ToList();

        foreach (var rarity in allowedRarities)
        {
            var monstersOfRarity = monsterDatabase.monsters.Where(m => m.monType == rarity).Count();
        }
    }

    public void RollGacha()
    {
        Debug.Log($"this is gacha cost = {gachaCost}");
        if (cdGacha) {return;}
       
        if (gachaResultPanel.coroutineFirework != null && gachaResultPanel.gameObject.activeInHierarchy && gachaResultPanel.chest.activeInHierarchy && gachaResultPanel.egg.activeInHierarchy )
        {
            ServiceLocator.Get<UIManager>().ShowMessage("You are click too fast!", 1f);
            return;
        }
        else if (totalPullCount >= 7)
        {
            ServiceLocator.Get<UIManager>().ShowMessage("Limit Gacha!", 1f);
            return;
        } 
        else if (!CoinManager.SpendCoins(gachaCost))
        {
            ServiceLocator.Get<UIManager>().ShowMessage("Not enough coins for gacha!", 1f);
            return;
        }
        
        drawGachaButton.interactable = false;
        cdGacha = true;
        onceEvent = true;

        // Di sini ngecek jumlah monster kurang dari 25
        if (MonsterManager.instance.activeMonsters.Count >= 25)
        {
            // monster nomber has reached its limit. Show an info message and then return
            Debug.Log("We have reached a limit of 25 monsters");
            TooltipManager.Instance.StartHover("You already have maximum number of monsters in this area.");
            StartCoroutine(EndHoverAfterDelay(4.0f));

            return;
        }

        // Increment total pull count
        totalPullCount++;
        IncrementAllPityCounters();
        UpdateGatchaCost();

        Debug.Log($"[Pull #{totalPullCount}] Rolling gacha for {gachaCost} coins...");

        MonsterType chosenRarity = GetRandomRarityWithPity(out float chosenRarityPercentage);
        MonsterDataSO selectedMonster = SelectRandomMonster(chosenRarity);

        if (selectedMonster == null)
        {
            ServiceLocator.Get<UIManager>().ShowMessage("No monsters available!", 1f);
            return;
        }

        Debug.Log($"[Pull #{totalPullCount}] Got {chosenRarity} - {selectedMonster.monsterName} ({chosenRarityPercentage:F2}% chance)");

        // Reset pity counter for the obtained rarity
        ResetPityCounter(chosenRarity);

        ShowGachaResult(selectedMonster, () => SellMonster(selectedMonster), () => SpawnMonster(selectedMonster));
        SfxCrackEgg ();

        
    }

    IEnumerator nCDGacha () {
        
        yield return new WaitForSeconds (6);
        ServiceLocator.Get<CoinDisplayUI>().UpdateCoinText();
        drawGachaButton.interactable = true;
        cdGacha = false;
    }

    private IEnumerator EndHoverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TooltipManager.Instance.EndHover();
    }

    private MonsterDataSO SelectRandomMonster(MonsterType rarity)
    {
        List<MonsterDataSO> candidates = monsterDatabase.monsters
            .Where(m => allowedRarities.Contains(m.monType))
            .Where(m => m.monType == rarity)
            .ToList();

        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
    }

    private MonsterType GetRandomRarityWithPity(out float resultPercentage)
    {
        resultPercentage = 100f;

        // Check for Hard Pity activation for any rarity
        foreach (var rarityWeight in rarityWeights)
        {
            if (!allowedRarities.Contains(rarityWeight.type)) continue;
            if (!rarityWeight.hasHardPity) continue;

            int pulls = GetPityCounter(rarityWeight.type);
            if (pulls >= rarityWeight.hardPityAt)
            {
                Debug.Log($"Hard Pity activated for {rarityWeight.type} at pull {pulls}!");
                resultPercentage = 100f;
                return rarityWeight.type;
            }
        }

        // Filter weights to only include allowed rarities
        var validWeights = rarityWeights.Where(w => allowedRarities.Contains(w.type)).ToList();
        if (validWeights.Count == 0)
        {
            resultPercentage = 100f;
            return allowedRarities[0];
        }

        // Apply soft pity and hard pity rate adjustments
        var adjustedWeights = ApplyPityAdjustments(validWeights);

        float totalWeight = adjustedWeights.Sum(r => r.weight);
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        MonsterType selectedType = adjustedWeights[0].type;

        foreach (var rarity in adjustedWeights)
        {
            cumulative += rarity.weight;
            if (roll <= cumulative)
            {
                selectedType = rarity.type;
                resultPercentage = (rarity.weight / totalWeight) * 100f;
                break;
            }
        }

        return selectedType;
    }

    private List<RarityWeight> ApplyPityAdjustments(List<RarityWeight> weights)
    {
        var adjustedWeights = new List<RarityWeight>();

        foreach (var weight in weights)
        {
            RarityWeight adjusted = new RarityWeight
            {
                type = weight.type,
                weight = weight.weight,
                hasSoftPity = weight.hasSoftPity,
                softPityStartAt = weight.softPityStartAt,
                softPityIncreasePercent = weight.softPityIncreasePercent,
                hasHardPity = weight.hasHardPity,
                hardPityAt = weight.hardPityAt,
                hardPityRateRange = weight.hardPityRateRange
            };

            int currentPulls = GetPityCounter(weight.type);

            // Soft Pity: Increase rarity rate progressively
            if (weight.hasSoftPity && currentPulls >= weight.softPityStartAt)
            {
                int pullsIntoSoftPity = currentPulls - weight.softPityStartAt;
                float increaseMultiplier = 1f + (pullsIntoSoftPity * weight.softPityIncreasePercent / 100f);
                adjusted.weight *= increaseMultiplier;
                Debug.Log($"Soft Pity active for {weight.type}! Pull {currentPulls}: rate boosted by {(increaseMultiplier - 1) * 100:F1}%");
            }

            // Hard Pity Rate Range: Apply special rate within range before guaranteed
            if (weight.hasHardPity && currentPulls >= weight.hardPityAt - 1 && currentPulls < weight.hardPityAt)
            {
                adjusted.weight = Random.Range(weight.hardPityRateRange.x, weight.hardPityRateRange.y);
                Debug.Log($"Hard Pity rate range applied for {weight.type} at pull {currentPulls}: {adjusted.weight:F1}");
            }

            adjustedWeights.Add(adjusted);
        }

        return adjustedWeights;
    }

    private int GetPityCounter(MonsterType type)
    {
        if (!pityCounters.ContainsKey(type))
        {
            pityCounters[type] = 0;
        }
        return pityCounters[type];
    }

    private void IncrementAllPityCounters()
    {
        foreach (var rarityWeight in rarityWeights)
        {
            if (!allowedRarities.Contains(rarityWeight.type)) continue;

            if (!pityCounters.ContainsKey(rarityWeight.type))
            {
                pityCounters[rarityWeight.type] = 0;
            }
            pityCounters[rarityWeight.type]++;
        }
        SaveAllCounters();
    }

    private void ResetPityCounter(MonsterType type)
    {
        if (pityCounters.ContainsKey(type))
        {
            pityCounters[type] = 0;
            SaveAllCounters();
            Debug.Log($"Pity counter reset for {type}");
        }
    }

    private void LoadPityCounters()
    {
        // Load total pull count
        totalPullCount = PlayerPrefs.GetInt(TOTAL_PULL_COUNT_KEY, 0);

        // Load pity counters per rarity
        pityCounters.Clear();
        foreach (var rarityWeight in rarityWeights)
        {
            string key = PITY_COUNTER_PREFIX + rarityWeight.type.ToString();
            pityCounters[rarityWeight.type] = PlayerPrefs.GetInt(key, 0);
        }
        UpdatePityCounterDisplay();
    }

    private void SaveAllCounters()
    {
        // Save total pull count
        PlayerPrefs.SetInt(TOTAL_PULL_COUNT_KEY, totalPullCount);

        // Save pity counters per rarity
        foreach (var kvp in pityCounters)
        {
            string key = PITY_COUNTER_PREFIX + kvp.Key.ToString();
            PlayerPrefs.SetInt(key, kvp.Value);
        }
        PlayerPrefs.Save();
        UpdatePityCounterDisplay();
    }

    private void UpdatePityCounterDisplay()
    {
        // Update total pulls display
        currentTotalPulls = totalPullCount;

        // Update pity counters per rarity display
        pityCounterDisplay.Clear();
        foreach (var kvp in pityCounters)
        {
            pityCounterDisplay.Add(new PityCounterDisplay
            {
                rarityType = kvp.Key,
                currentPullCount = kvp.Value
            });
        }
    }

    private void UpdateGatchaCost()
    {
        // Update Gacha Cost
        if (currentTotalPulls < 1)
        {
            PlayerPrefs.SetInt("gachaPrice", 0);
            gachaCost = PlayerPrefs.GetInt("gachaPrice");

            gachaPriceText.text = "FREE!";
            return;
        }
        else if (currentTotalPulls == 1)
        {
            PlayerPrefs.SetInt("gachaPrice", 200);
        }
        else if (currentTotalPulls < 7)
        {
            PlayerPrefs.SetInt("gachaPrice", PlayerPrefs.GetInt("gachaPrice") + 50);
        }
        else if (currentTotalPulls >= 7)
        {
            PlayerPrefs.SetInt("gachaPrice", 0);

            if (totalPullCount == 0)
            {
                gachaPriceText.text = "Free";
                buyImage.material = null;
                coinImage.enabled = false;
            }
            else
            {
                buyImage.material = grayscaleMaterial;
                gachaPriceText.text = "";
                coinImage.enabled = false;
            }
            return;
        }

        gachaCost = PlayerPrefs.GetInt("gachaPrice");
        Debug.Log($"Gacha Cost Update = {gachaCost}");
        gachaPriceText.text = gachaCost.ToString();
    }

    private void OnValidate()
    {
        // Update display when in editor
        if (Application.isPlaying && pityCounters != null && pityCounters.Count > 0)
        {
            UpdatePityCounterDisplay();
        }
    }

    private void SpawnMonster(MonsterDataSO monsterData)
    {
        if (onceEvent) {
            onceEvent = false;
            ServiceLocator.Get<MonsterManager>().SpawnMonster(monsterData);

            // Update List Monster Catalogue
            if (ServiceLocator.Get<MonsterCatalogueListUI>() != null)
                ServiceLocator.Get<MonsterCatalogueListUI>().RefreshCatalogue();
        }
    }

    private void SellMonster(MonsterDataSO monsterData)
    {
        if (onceEvent) {
            onceEvent = false;
            ServiceLocator.Get<MonsterManager>().SellMonster(monsterData);
        }
        
    }

    private void ShowGachaResult(MonsterDataSO monster, System.Action onSellComplete, System.Action onSpawnComplete)
    {
        if (gachaResultPanel != null && onceEvent)
        {
            
            gachaResultPanel.Show(monster, onSellComplete, onSpawnComplete);
        }
        StartCoroutine (nCDGacha ());
    }

    #region Audio
   void SfxCrackEgg () {
      ServiceLocator.Get<AudioManager> ().PlaySFX (openEggClip);
   }
   #endregion
}