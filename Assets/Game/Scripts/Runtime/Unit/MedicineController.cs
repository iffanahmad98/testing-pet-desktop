using UnityEngine;

public class MedicineController : MonoBehaviour, IConsumable
{
    private ItemDataSO itemData;
    public event System.Action OnPlaced;

    public void Initialize(ItemDataSO data)
    {
        itemData = data;
    }

    public void Consume(MonsterController monster)
    {
        // monster.Heal(itemData.fullness); // Or any other stat
        ServiceLocator.Get<MonsterManager>().DespawnToPool(gameObject);
    }

    public ItemDataSO GetItemData() => itemData;
}
