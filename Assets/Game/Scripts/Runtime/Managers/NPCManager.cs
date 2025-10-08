using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public GameObject NPCIdleFlower;
    private void Awake()
    {
        ServiceLocator.Register<NPCManager>(this);
    }

    public GameObject GetIdleTarget(int npcIndex)
    {
        GameObject target;
        Debug.Log($"NPC INDEX {npcIndex}");
        target = NPCIdleFlower.GetComponent<NPCIdleFlower>().GetIdleStation(npcIndex);
        
        return target;
    }
}
