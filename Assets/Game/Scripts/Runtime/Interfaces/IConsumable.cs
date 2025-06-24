public interface IConsumable
{
    void Initialize(ItemDataSO itemData);
    void Consume(MonsterController monster);
    ItemDataSO GetItemData();
    event System.Action OnPlaced;
}
