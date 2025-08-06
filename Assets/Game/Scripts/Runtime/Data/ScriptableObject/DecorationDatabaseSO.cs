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
}
