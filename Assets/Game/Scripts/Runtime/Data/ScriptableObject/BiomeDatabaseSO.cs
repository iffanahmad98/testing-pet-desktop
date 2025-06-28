using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeDatabaseSO", menuName = "Scriptable Objects/BiomeDatabaseSO")]
public class BiomeDatabaseSO : ScriptableObject
{
    public List<BiomeDataSO> allBiomes;

    public BiomeDataSO GetBiomeByName(string name)
    {
        return allBiomes.Find(biome => biome.biomeName == name);
    }
}
