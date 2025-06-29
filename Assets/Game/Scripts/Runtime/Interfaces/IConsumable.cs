using UnityEngine;

public interface IConsumable
{
    void Initialize(ItemDataSO itemData, RectTransform groundRect = null);
    void Consume(MonsterController monster);
    ItemDataSO GetItemData();
    event System.Action OnPlaced;
}
