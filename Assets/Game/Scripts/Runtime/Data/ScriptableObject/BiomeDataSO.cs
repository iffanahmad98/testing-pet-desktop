using UnityEngine;

[CreateAssetMenu(fileName = "New Biome", menuName = "Biome/Biome Data")]
public class BiomeDataSO : ScriptableObject
{

    [Header("Metadata")]
    public string biomeID = "biome_default";
    public string biomeName = "New Biome";
    public string description = "Description here.";
    public int price = 777;
    public Sprite thumbnail; // Used in shop card UI

    [Header("Buying Requierements")]
    public MonsterRequirements[] monsterRequirements;

    [Header("Sky Layer")]
    public Sprite skyBackground;
    public GameObject[] skyObjects;

    [Header("Cloud Settings")]
    public Sprite[] cloudSprites;
    public float cloudSpawnRate = 3f; // Time between spawns in seconds
    public float cloudMinSpeed = 20f; // Minimum movement speed
    public float cloudMaxSpeed = 50f; // Maximum movement speed
    public float cloudMinScale = 0.5f; // Minimum cloud scale
    public float cloudMaxScale = 1.5f; // Maximum cloud scale
    public int maxClouds = 8; // Maximum clouds visible at once

    [Header("Effect Layer")]
    public GameObject[] effectPrefabs;

    [Header("Ambient Layer")]
    public Sprite ambientBackground;

    [Header("Ground Layer")]
    public Sprite groundBackground;
    public Color groundFilterColor;
    public float groundFilterAlpha;
}
