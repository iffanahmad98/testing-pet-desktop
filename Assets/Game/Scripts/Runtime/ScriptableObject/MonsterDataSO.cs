using UnityEngine;
using Spine.Unity;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Monster/Monster Data")]
public class MonsterDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string monsterName;              
    public string id;               
    public int monPrice = 10;      

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
    

    [Header("Happiness Settings")]
    public float areaHappinessRate = 0.2f;
    public float pokeHappinessValue = 2f;
    public float hungerHappinessThreshold = 20f; // New field - threshold below which hunger affects happiness
    public float hungerHappinessDrainRate = 2f; // New field - how much happiness drains when hungry

    [Header("Evolution")]
    public bool canEvolve = true;
    public bool isEvolved = false;
    public bool isFinalEvol = false;
    public int evolutionLevel = 0;

    [Header("Evolution Requirements")]
    [Tooltip("Required: Each monster must have its own evolution requirements")]
    public EvolutionRequirementsSO evolutionRequirements;

    [Header("Spine Data")]
    public SkeletonDataAsset[] monsterSpine;

    [Header("Images")]
    public Sprite[] monsImgs;           // [0] base, [1+] evolved forms
    public Sprite[] monIcons;          // [0] base, [1+] evolved forms

    [Header("Sound Effects")]
    public AudioClip[] idleSounds;      // Randomly played during idle state
    public AudioClip happySound;       // Played when happiness increases
    public AudioClip eatSound;         // Played when eating
    public AudioClip hurtSound;       // Played when taking damage
    public AudioClip evolveSound;     // Played during evolution
    public AudioClip deathSound;     // Played when monster dies
    public AudioClip interactionSound; // Played when player interacts

    [Header("Evolution Animations")]
    public EvolutionAnimationSet[] evolutionAnimationSets;

    [Header("Evolution Behaviors")]
    public EvolutionBehaviorConfig[] evolutionBehaviors;
}

[System.Serializable]
public class EvolutionBehaviorConfig
{
    public int evolutionLevel;
    public MonsterBehaviorConfigSO behaviorConfig;
}

[System.Serializable]
public class EvolutionAnimationSet
{
    public int evolutionLevel;
    public string[] availableAnimations;
}
