namespace MagicalGarden.Inventory
{
    [System.Serializable]
    public class InventoryItem
    {
        public ItemData itemData;
        public int quantity;

        public InventoryItem(ItemData data, int qty)
        {
            itemData = data;
            quantity = qty;
        }
    }
}