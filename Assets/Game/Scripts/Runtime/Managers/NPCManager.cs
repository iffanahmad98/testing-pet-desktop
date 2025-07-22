using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public GameObject NPCIdleFlower;
    private void Awake()
    {
        ServiceLocator.Register<NPCManager>(this);
    }

    public GameObject GetIdlePos(int npcIndex)
    {
        GameObject position;
        if (npcIndex == 0)
        {
            position = NPCIdleFlower.transform.GetChild(0).gameObject; // Assuming the first child is NPC1
        }
        else if (npcIndex == 1)
        {
            position = NPCIdleFlower.transform.GetChild(1).gameObject; // Assuming the second child is NPC2
        }
        else
        {
            position = null; // Default position if index is invalid
        }

        return position;
    }
}
