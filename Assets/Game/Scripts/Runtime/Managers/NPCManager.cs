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
        if (npcIndex == 0)
        {
            target = NPCIdleFlower.GetComponent<NPCIdleFlower>().GetIdleStation(npcIndex);
        }
        else if (npcIndex == 1)
        {
            target = NPCIdleFlower.GetComponent<NPCIdleFlower>().GetIdleStation(npcIndex);
        }
        else
        {
            target = null; // Default position if index is invalid
        }
        return target;
    }
}
