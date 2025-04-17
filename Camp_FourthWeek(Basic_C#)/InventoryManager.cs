namespace Camp_FourthWeek_Basic_C__;

public class InventoryManager
{
    private static InventoryManager instance;

    public static InventoryManager Instance
    {
        get
        {
            if (instance == null) instance = new InventoryManager();
            return instance;
        }
    }

    public List<Item> Inventory { get; } = new();

    public void AddItem(Item _item)
    {
        Inventory.Add(_item);
    }

    public void RemoveItem(Item _item)
    {
        if (EquipmentManager.IsEquipped(_item))
            EquipmentManager.UnequipItem(_item.ItemType);

        Inventory.Remove(_item);
    }
}