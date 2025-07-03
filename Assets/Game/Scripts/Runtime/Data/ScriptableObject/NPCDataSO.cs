using UnityEngine;
using Spine.Unity;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewNPC", menuName = "NPC/NPC Data")]
public class NPCDataSO : ScriptableObject
{
    public string npcName;
    public int npcID;
    public string npcDescription;

    public SkeletonDataAsset[] skeletonDataAssets;
    public Sprite[] npcPortraits;

    public Action<MonsterController, float> onMonsterDataChanged;

    public void DoAction(MonsterController controller, float value)
    {
        onMonsterDataChanged?.Invoke(controller, value);
    }
 
}
