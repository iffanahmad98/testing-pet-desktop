using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DecorationDatabase", menuName = "BumiMobile/Decoration Database")]
public class DecorationDatabaseSO : ScriptableObject
{
    public List<DecorationDataSO> allDecorations = new List<DecorationDataSO>();
    public DecorationDataSO GetDecorationByID(string id)
    {
        return allDecorations.Find(decoration => decoration.decorationID == id);
    }

    public int GetTotalAllDecorations () { // RewardAnimator.cs 
        return allDecorations.Count;
    }

    public DecorationDataSO GetRandomAvailableDecorationSO () { // RewardAnimator.cs
        PlayerConfig playerConfig = SaveSystem.PlayerConfig;
        List <DecorationDataSO> listAvailableDecorations = new List <DecorationDataSO> ();
        foreach (DecorationDataSO so in allDecorations) {
            listAvailableDecorations.Add (so);
        }

        for (int i = listAvailableDecorations.Count - 1; i >= 0; i--)
        {
            DecorationDataSO so = listAvailableDecorations[i];

            if (playerConfig.HasDecoration(so.decorationID))
            {
                listAvailableDecorations.RemoveAt(i);
            }
        }
        return listAvailableDecorations [UnityEngine.Random.Range (0,listAvailableDecorations.Count)];
    }
}
