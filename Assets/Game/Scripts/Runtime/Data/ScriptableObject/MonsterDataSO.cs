using System;
using UnityEngine;
using Spine.Unity;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Monster/Monster Data")]
public class MonsterDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string monsterName;
    public string id;
    public string description;

    [Header("Classification")]
    public MonsterType monType = MonsterType.Common; // Type of monster (Common, Rare, etc.)
    public PoopType poopType = PoopType.Normal; // Type of poop this monster produces

    [Header("Stats")]
    public float moveSpd = 100f;       // Move speed
    public float foodDetectionRange = 100f;     // Range to detect food
    public float eatDistance = 5f;              // Distance to eat food
    public float baseHappiness = 100f;            // Add base happiness
    public float baseHunger = 100f;               // Add base hunger
    public float hungerDepleteRate = 0.1f;  // How fast hunger depletes
    public float maxHealthStage1 = 0f;
    public float maxHealthStage2 = 0f;
    public float maxHealthStage3 = 0f;
    public float maxNutritionStage1 = 0f;          // Stage 1 nutrition
    public float maxNutritionStage2 = 0f;          // Stage 2 nutrition
    public float maxNutritionStage3 = 0f;          // Stage 3 nutrition


    [Header("Drop Rates")]
    public float poopRate = 20f;     // Default: 20 minutes
    public float goldCoinDropRateStage1 = 2f;    // Stage 1 gold coin rate
    public float platCoinDropRateStage1 = 60f;  // Stage 1 silver coin rate
    public float goldCoinDropRateStage2 = 0f;   // Stage 2 gold coin rate
    public float platCoinDropRateStage2 = 0f; // Stage 2 silver coin rate
    public float goldCoinDropRateStage3 = 0f;   // Stage 3 gold coin rate
    public float platCoinDropRateStage3 = 0f; // Stage 3 silver coin rate

    [Header("Gacha Settings")]
    public float gachaChancePercent = 0f;  // e.g., 0.10 for 0.10%
    public string gachaChanceDisplay = ""; // e.g., "0.10%" for display
    public bool isGachaOnly = false;       // True if buy price is 0

    [Header("Happiness Settings")]
    public float areaHappinessRate = 0.1f; // Rate at which happiness increases in the area
    public float pokeHappinessValue = 5f;
    public float hungerHappinessThreshold = 40f; // threshold below which hunger affects happiness
    public float hungerHappinessDrainRate = 0.1f; // how much happiness drains when hungry

    [Header("Evolution")]
    public bool canEvolve = true;
    public bool isEvolved = false;
    public int evolutionLevel = 1;

    // Helper method to get evolution stage name
    public string GetEvolutionStageName(int level)
    {
        switch (level)
        {
            case 1:
                return "Buds";
            case 2:
                return "Bloom";
            case 3:
                return "Flourish";
            default:
                return "Other";
        }
    }

    [Header("Evolution Requirements - Embedded")]
    public EvolutionRequirement[] evolutionRequirements;

    [Header("Spine Data")]
    public SkeletonDataAsset[] monsterSpine;

    [Header("Images")]
    public Sprite[] CardIcon;
    public Sprite[] DetailsIcon;
    public Sprite[] CatalogueIcon;

    [Header("Evolution Animations")]
    public EvolutionAnimationSet[] evolutionAnimationSets;

    [Header("Evolution Behaviors")]
    public EvolutionBehaviorConfig[] evolutionBehaviors;

    [Header("Pricing")]
    public int monsterPrice = 10; // Buy price (Stage 1 only)
    public int sellPriceStage1 = 0;  // Stage 1 sell price
    public int sellPriceStage2 = 0;  // Stage 2 sell price  
    public int sellPriceStage3 = 0;  // Stage 3 sell price

    [Header("Sound Effects")]
    public AudioClip[] idleSounds;      // Randomly played during idle state
    public AudioClip happySound;       // Played when happiness increases
    public AudioClip eatSound;         // Played when eating
    public AudioClip hurtSound;       // Played when taking damage
    public AudioClip evolveSound;     // Played during evolution
    public AudioClip deathSound;     // Played when monster dies
    public AudioClip interactionSound; // Played when player interacts


    public Sprite GetEvolutionIcon(int evolutionLevel, MonsterIconType iconType)
    {
        switch (evolutionLevel)
        {
            case 1:
                if (iconType == MonsterIconType.Card && CardIcon.Length > 0) return CardIcon[0];
                if (iconType == MonsterIconType.Catalogue && CatalogueIcon.Length > 0) return CatalogueIcon[0];
                if (iconType == MonsterIconType.Detail && DetailsIcon.Length > 0) return DetailsIcon[0];
                break;
            case 2:
                if (iconType == MonsterIconType.Card && CardIcon.Length > 1) return CardIcon[1];
                if (iconType == MonsterIconType.Catalogue && CatalogueIcon.Length > 1) return CatalogueIcon[1];
                if (iconType == MonsterIconType.Detail && DetailsIcon.Length > 1) return DetailsIcon[1];
                break;
            case 3:
                if (iconType == MonsterIconType.Card && CardIcon.Length > 2) return CardIcon[2];
                if (iconType == MonsterIconType.Catalogue && CatalogueIcon.Length > 2) return CatalogueIcon[2];
                if (iconType == MonsterIconType.Detail && DetailsIcon.Length > 2) return DetailsIcon[2];
                break;
            default:
                if (iconType == MonsterIconType.Card && CardIcon.Length > 0) return CardIcon[0];
                if (iconType == MonsterIconType.Catalogue && CatalogueIcon.Length > 0) return CatalogueIcon[0];
                if (iconType == MonsterIconType.Detail && DetailsIcon.Length > 0) return DetailsIcon[0];
                return null; // Default to first icon if not found
        }
        return null; // Return null if no icon found for the specified evolution level and type
    }

    public int GetSellPrice(int currentEvolutionLevel)
    {
        switch (currentEvolutionLevel)
        {
            case 1: return sellPriceStage1;
            case 2: return sellPriceStage2;
            case 3: return sellPriceStage3;
            default: return sellPriceStage1;
        }
    }

    public float GetGoldCoinDropRate(int evolutionLevel)
    {
        switch (evolutionLevel)
        {
            case 1: return goldCoinDropRateStage1;
            case 2: return goldCoinDropRateStage2 > 0 ? goldCoinDropRateStage2 : goldCoinDropRateStage1;
            case 3: return goldCoinDropRateStage3 > 0 ? goldCoinDropRateStage3 : goldCoinDropRateStage1;
            default: return goldCoinDropRateStage1;
        }
    }

    public float GetPlatinumCoinDropRate(int evolutionLevel)
    {
        switch (evolutionLevel)
        {
            case 1: return platCoinDropRateStage1;
            case 2: return platCoinDropRateStage2 > 0 ? platCoinDropRateStage2 : platCoinDropRateStage1;
            case 3: return platCoinDropRateStage3 > 0 ? platCoinDropRateStage3 : platCoinDropRateStage1;
            default: return platCoinDropRateStage1;
        }
    }

    public float GetMaxNutrition(int evolutionLevel)
    {
        switch (evolutionLevel)
        {
            case 1: return maxNutritionStage1 > 0 ? maxNutritionStage1 : 100f; // Default to 100 if not set
            case 2: return maxNutritionStage2 > 0 ? maxNutritionStage2 : 0f;
            case 3: return maxNutritionStage3 > 0 ? maxNutritionStage3 : 0f;
            default: return maxNutritionStage1;
        }
    }
    public float GetMaxHealth(int evolutionLevel)
    {
        switch (evolutionLevel)
        {
            case 1: return maxHealthStage1 > 0 ? maxHealthStage1 : 100f; // Default to 100 if not set
            case 2: return maxHealthStage2 > 0 ? maxHealthStage2 : 0f;
            case 3: return maxHealthStage3 > 0 ? maxHealthStage3 : 0f;
            default: return maxHealthStage1;
        }
    }
}

[Serializable]
public class MonsterSaveData
{
    [Header("Identity")]
    public string instanceId;
    public string monsterId;
    public int gameAreaId;

    [Header("Core Stats")]
    public float currentHealth;
    public float currentHunger;
    public float currentHappiness;
    // public float currentFullness;

    [Header("Evolution Data")]
    public int currentEvolutionLevel;
    public int currentInteraction;
    public int nutritionConsumed;
    public float totalTimeSinceCreation;
    public string timeCreated;
    // public EvolutionProgressData evolutionProgress;
}

[Serializable]
public class EvolutionProgressData
{
    [Tooltip("Total time alive in days")]
    public float totalDaysSinceCreation;

    [Tooltip("Has reached maximum fullness (per-evolutionLevel)")]
    public bool isFullnessMaxed;
}

[Serializable]
public class EvolutionRequirement
{
    [Header("Target Evolution")]
    public int targetEvolutionLevel = 2;

    [Header("Time Requirements")]
    public float minTimeAlive = 3f;

    [Header("Current Status Requirements (Dynamic)")]
    [Range(0f, 100f)] public float minCurrentHappiness = 50f;
    [Range(0f, 100f)] public float minCurrentHunger = 50f;

    [Header("Accumulated Progress Requirements")]
    public int minFoodConsumed = 1;
    public int minInteractions = 1;

    [Header("Custom Conditions")]
    public Func<MonsterController, bool> customCondition;

    [Header("Evolution Info")]
    public string evolutionName = "Evolution";
    public string description = "Evolution requirements";

    [Header("Reset Behavior")]
    [Range(0f, 1f)] public float progressRetentionPercentage = 0f;
}

[Serializable]
public class EvolutionAnimationSet
{
    public int evolutionLevel;
    public string[] availableAnimations;
}

[Serializable]
public class EvolutionBehaviorConfig
{
    public int evolutionLevel;
    public MonsterBehaviorConfigSO behaviorConfig;
}

public enum MonsterIconType
{
    Card,
    Catalogue,
    Detail
}

