using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class MonsterManagerEligible : MonoBehaviour
{
    public static MonsterManagerEligible Instance;
    PlayerConfig playerConfig;
    [SerializeField] List <MonsterDataSO> listMonsterDataSO = new ();
    [SerializeField] MonsterDatabaseSO monsterDatabase;
    [SerializeField] List <string> monsterId = new ();
    void Awake () {
        Instance = this;
    }

    void Start () 
    {
       playerConfig = SaveSystem.PlayerConfig;
       Invoke ("RefreshListMonsterDataSO", 0.2f);
    }

    public int GetTotalMonstersEqualRequirements(bool anyRequirements, MonsterType monsterType = MonsterType.Common)
    { // EligiblePetMonster.cs
        /*
        listMonsterDataSO.Clear ();
        foreach (MonsterSaveData data in playerConfig.ownedMonsters) {
            MonsterDataSO dataSO = monsterDatabase.GetMonsterByID (data.monsterId);
            listMonsterDataSO.Add (dataSO);
        }
        */
        int result = 0;
        foreach (MonsterDataSO monsterDataSO in listMonsterDataSO)
        {
            if (anyRequirements)
            {
                result++;
            }
            else
            {
                if (monsterDataSO.monType == monsterType)
                {
                    result++;
                }
            }
        }

        return result;
    }

    void RefreshListMonsterDataSO () 
    {
        listMonsterDataSO.Clear ();
        Debug.Log ("Player Config Owned Monsters " + playerConfig.ownedMonsters.Count);

        foreach (MonsterSaveData data in playerConfig.ownedMonsters)
        {
            MonsterDataSO dataSO = monsterDatabase.GetMonsterByID(data.monsterId);
            monsterId.Add (data.instanceId);
            listMonsterDataSO.Add(dataSO);
        }
        /*
        foreach (MonsterDataSO dataSO in MonsterManager.instance.GetListPurchasedMonsterDataSO())
        {
            listMonsterDataSO.Add(dataSO);
        }
        */
    }

    public void AddListMonsterEligible(MonsterDataSO dataSO, string instanceId)
    {
        if (!monsterId.Contains (instanceId)) {
            listMonsterDataSO.Add(dataSO);
            monsterId.Add (instanceId);
        }
    }

    #region Utility
    public List <MonsterDataSO> GetListMonsterDataSO () { // MonsterShopManager.cs
     //   Debug.Log ("Monster Data SO : " +  listMonsterDataSO);
        return listMonsterDataSO;
    }
    #endregion

}
