using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MonsterDatabase", menuName = "Monster/Monster Database")]
public class MonsterDatabaseSO : ScriptableObject
{
    public List<MonsterDataSO> monsters;

    public MonsterDataSO GetMonsterByID(string id)
    {
        id = id.ToLower();
        if (!monsters.Find(monster => monster.id == id)) {
            Debug.LogError ("Not found id : " +id);
           // ShowAllMonstersId ();
        }

        return monsters.Find(monster => monster.id == id);
    }

    void ShowAllMonstersId () {
        foreach (MonsterDataSO data in monsters) {
            Debug.Log ("found id :" + data.id);
        }
    }

    public List<MonsterDataSO> GetMonstersByType(MonsterType type)
    {
        return monsters.FindAll(monster => monster.monType == type);
    }

    
}
