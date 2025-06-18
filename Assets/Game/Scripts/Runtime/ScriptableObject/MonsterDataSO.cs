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
    public float maxHealth = 100f;        // Maximum health
    public float moveSpd = 100f;       // Move speed
    public float hungerDepleteRate = 0.05f;  // How fast hunger depletes
    public float poopRate = 20f;     // Default: 20 minutes
    public float goldCoinDropRate = 60f;    // Default: 60 minutes for gold coins
    public float silverCoinDropRate = 30f;  // Default: 30 minutes for silver coins
    public float maxHunger = 200f;      // Maximum hunger
    public float baseHunger = 50f;     // Add base hunger
    public float baseHappiness = 0f;   // Add base happiness
    public float foodDetectionRange = 200f; // Range to detect food
    public float eatDistance = 5f;      // Distance to eat food

    [Header("Gacha Settings")]
    public float gachaChancePercent = 0f;  // e.g., 0.10 for 0.10%
    public string gachaChanceDisplay = ""; // e.g., "0.10%" for display
    public bool isGachaOnly = false;       // True if buy price is 0


    [Header("Happiness Settings")]
    public float areaHappinessRate = 0.2f;
    public float pokeHappinessValue = 2f;
    public float hungerHappinessThreshold = 20f; // New field - threshold below which hunger affects happiness
    public float hungerHappinessDrainRate = 2f; // New field - how much happiness drains when hungry

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

    /// <summary>
    /// Get the sell price based on current evolution level
    /// </summary>
    /// <param name="currentEvolutionLevel">Current evolution level (1, 2, or 3)</param>
    /// <returns>Sell price for the current evolution stage</returns>
    public int GetSellPrice(int currentEvolutionLevel)
    {
        switch (currentEvolutionLevel)
        {
            case 1: return sellPriceStage1;
            case 2: return sellPriceStage2;
            case 3: return sellPriceStage3;
            default: return sellPriceStage1; // Fallback to stage 1
        }
    }
    
    /// <summary>
    /// Get the sell price for the highest available evolution stage
    /// </summary>
    /// <returns>Highest sell price available</returns>
    public int GetMaxSellPrice()
    {
        if (sellPriceStage3 > 0) return sellPriceStage3;
        if (sellPriceStage2 > 0) return sellPriceStage2;
        return sellPriceStage1;
    }
}

[Serializable]
public class EvolutionRequirement
{
    [Header("Target Evolution")]
    public int targetEvolutionLevel = 1;
    
    [Header("Time Requirements")]
    public float minTimeAlive = 300f; 
    
    [Header("Current Status Requirements (Dynamic)")]
    [Range(0f, 100f)] public float minCurrentHappiness = 80f;
    [Range(0f, 100f)] public float minCurrentHunger = 70f;

    [Header("Accumulated Progress Requirements")]
    public int minFoodConsumed = 10;
    public int minInteractions = 20;

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
