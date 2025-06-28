using System;
using UnityEngine;
using Spine.Unity;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Monster/Monster Data")]
public class MonsterDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string monsterName;
    public string id;

    [Header("Classification")]
    public MonsterType monType = MonsterType.Common; // Type of monster (Common, Rare, etc.)
    public PoopType poopType = PoopType.Normal; // Type of poop this monster produces

    [Header("Stats")]
    public float hp = 100f;        // Maximum health
    public float moveSpd = 100f;       // Move speed
    public float foodDetectionRange = 200f;     // Range to detect food
    public float eatDistance = 5f;              // Distance to eat food
    public float startingHappiness = 50f;            // Add base happiness
    public float startingHunger = 100f;               // Add base hunger
    public float hungerDepleteRate = 0.001f;  // How fast hunger depletes
    public float maxNutritionStage1 = 0f;          // Stage 1 hunger (keep existing)
    public float maxNutritionStage2 = 0f;          // Stage 2 hunger
    public float maxNutritionStage3 = 0f;          // Stage 3 hunger


    [Header("Drop Rates")]
    public float poopRate = 20f;     // Default: 20 minutes
    public float goldCoinDropRateStage1 = 2f;    // Stage 1 gold coin rate
    public float silverCoinDropRateStage1 = 60f;  // Stage 1 silver coin rate
    public float goldCoinDropRateStage2 = 0f;   // Stage 2 gold coin rate
    public float silverCoinDropRateStage2 = 0f; // Stage 2 silver coin rate
    public float goldCoinDropRateStage3 = 0f;   // Stage 3 gold coin rate
    public float silverCoinDropRateStage3 = 0f; // Stage 3 silver coin rate

    [Header("Gacha Settings")]
    public float gachaChancePercent = 0f;  // e.g., 0.10 for 0.10%
    public string gachaChanceDisplay = ""; // e.g., "0.10%" for display
    public bool isGachaOnly = false;       // True if buy price is 0

    [Header("Happiness Settings")]
    public float areaHappinessRate = 0.1f; // Rate at which happiness increases in the area
    public float pokeHappinessValue = 5f;
    public float hungerHappinessThreshold = 30f; // threshold below which hunger affects happiness
    public float hungerHappinessDrainRate = 0.1f; // how much happiness drains when hungry

    [Header("Evolution")]
    public bool canEvolve = true;
    public bool isEvolved = false;
    public bool isFinalEvol = false;
    public int evolutionLevel = 1;

    [Header("Evolution Requirements - Embedded")]
    public EvolutionRequirement[] evolutionRequirements;

    [Header("Spine Data")]
    public SkeletonDataAsset[] monsterSpine;

    [Header("Images")]
    public Sprite[] monsIconImg;

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

    public float GetSilverCoinDropRate(int evolutionLevel)
    {
        switch (evolutionLevel)
        {
            case 1: return silverCoinDropRateStage1;
            case 2: return silverCoinDropRateStage2 > 0 ? silverCoinDropRateStage2 : silverCoinDropRateStage1;
            case 3: return silverCoinDropRateStage3 > 0 ? silverCoinDropRateStage3 : silverCoinDropRateStage1;
            default: return silverCoinDropRateStage1;
        }
    }

    public float GetMaxHunger(int evolutionLevel)
    {
        switch (evolutionLevel)
        {
            case 1: return maxNutritionStage1 > 0 ? maxNutritionStage1 : 100f; // Default to 100 if not set
            case 2: return maxNutritionStage2 > 0 ? maxNutritionStage2 : maxNutritionStage1 * 2f;
            case 3: return maxNutritionStage3 > 0 ? maxNutritionStage3 : maxNutritionStage2 * 1.5f;
            default: return maxNutritionStage1;
        }
    }
}

[Serializable]
public class MonsterSaveData
{
    public string monsterId;
    public float lastHunger;
    public float lastHappiness;
    public float lastLowHungerTime;
    public bool isSick;
    public int evolutionLevel;
    public float timeSinceCreation;
    public int nutritionCount;
    public int interactionCount;
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
    public bool resetHappinessProgress = true;
    public bool resetHungerProgress = true;
    public bool resetFoodProgress = true;
    public bool resetInteractionProgress = true;
    [Range(0f, 1f)] public float progressRetentionPercentage = 0f;
}

[Serializable]
public class EvolutionBehaviorConfig
{
    public int evolutionLevel;
    public MonsterBehaviorConfigSO behaviorConfig;
}

[Serializable]
public class EvolutionAnimationSet
{
    public int evolutionLevel;
    public string[] availableAnimations;
}
