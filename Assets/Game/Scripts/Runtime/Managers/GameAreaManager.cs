using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameArea
{
    public string name;
    public int index;
    public bool isActive;
    public string activeBiomeID = "default_biome";
    public bool isSkyEnabled = false;
    public bool isCloudEnabled = false;
    public bool isAmbientEnabled = false;
    public bool isRainEnabled = false;
    public List<MonsterController> monsters = new List<MonsterController>();
    public List<NPCController> npcMonsters = new List<NPCController>();
    public List<CoinController> coins = new List<CoinController>();
    public List<PoopController> poops = new List<PoopController>();
}

public class GameAreaManager : MonoBehaviour
{
    [SerializeField] private List<GameArea> gameAreas;

    public void SetActiveGameArea(int index)
    {
        for (int i = 0; i < gameAreas.Count; i++)
        {
            gameAreas[i].isActive = i == index;
        }
    }

    public GameArea GetActiveGameArea()
    {
        return gameAreas.Find(area => area.isActive);
    }

    public void RenameGameArea(int index, string newName)
    {
        if (index < 0 || index >= gameAreas.Count) return;
        gameAreas[index].name = newName;
    }

}
