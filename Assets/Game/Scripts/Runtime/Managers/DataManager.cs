using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField] private MonsterDatabaseSO monsterDatabase;
    [SerializeField] private ItemDatabaseSO itemDatabase;
    [SerializeField] private BiomeDatabaseSO biomeDatabase;
    [SerializeField] private FacilityDatabaseSO facilityDatabase;
    [SerializeField] private MonsterDatabaseSO npcMonsterDatabase;

    void Start()
    {
        ServiceLocator.Register<DataManager>(this);
    }

    public MonsterDatabaseSO GetMonsterDatabase() => monsterDatabase;
    public MonsterDataSO GetMonsterData(string id) => monsterDatabase.GetMonsterByID(id);
    public ItemDatabaseSO GetItemDatabase() => itemDatabase;
    public ItemDataSO GetItemData(string id) => itemDatabase.GetItem(id);
    public FacilityDatabaseSO GetFacilityDatabase() => facilityDatabase;
    public FacilityDataSO GetFacilityData(string id) => facilityDatabase.GetFacility(id);
    public BiomeDatabaseSO GetBiomeDatabase() => biomeDatabase;
    public BiomeDataSO GetBiomeData(string name) => biomeDatabase.GetBiomeByName(name);
    public MonsterDatabaseSO GetNPCMonsterDatabase() => npcMonsterDatabase;
    public MonsterDataSO GetNPCMonsterData(string id) => npcMonsterDatabase.GetMonsterByID(id);
}
