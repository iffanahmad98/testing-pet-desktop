using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class MonsterManagerEligible : MonoBehaviour
{
    public static MonsterManagerEligible Instance;
    PlayerConfig playerConfig;
    [SerializeField] List <MonsterDataSO> listMonsterDataSO = new ();
    [SerializeField] MonsterDatabaseSO monsterDatabase;

    void Awake () {
        Instance= this;
    }
    void Start () {
        playerConfig = SaveSystem.PlayerConfig;
        MonsterManager.instance.AddEventPetMonsterChanged (RefreshListMonsterDataSO);
        Invoke ("RefreshListMonsterDataSO", 0.5f);
    }
    void nStart () {
        RefreshListMonsterDataSO ();
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

    void RefreshListMonsterDataSO () {
        listMonsterDataSO.Clear ();
        //foreach (MonsterSaveData data in playerConfig.ownedMonsters) {
        //    MonsterDataSO dataSO = monsterDatabase.GetMonsterByID (data.monsterId);
        //    listMonsterDataSO.Add (dataSO);
        //}
        foreach (MonsterDataSO dataSO in MonsterManager.instance.GetListPurchasedMonsterDataSO ()) {
            listMonsterDataSO.Add (dataSO);
        }
        
    }

    #region Utility
    public List <MonsterDataSO> GetListMonsterDataSO () { // MonsterShopManager.cs
        Debug.Log ("Monster Data SO : " +  listMonsterDataSO);
        return listMonsterDataSO;
    }
    #endregion

}
