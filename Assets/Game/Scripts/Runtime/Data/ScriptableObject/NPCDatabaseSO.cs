using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NPCDatabase", menuName = "Database/NPCDatabase")]
public class NPCDatabaseSO : ScriptableObject
{
    public List<NPCDataSO> allNPC;

    public NPCDataSO GetNPCByName(string name)
    {
        return allNPC.Find(npc => npc.npcName == name);
    }
}
