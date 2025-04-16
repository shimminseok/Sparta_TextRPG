using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Camp_FourthWeek_Basic_C__;
namespace Camp_FourthWeek_Basic_C__
{
    internal static class InventoryManager
    {
        public static List<Item> Inventory { get; private set; } = new List<Item>();
        public static void AddItem(Item _item)
        {
            Inventory.Add(_item);
        }
        public static void RemoveItem(Item _item)
        {
            Inventory.Remove(_item);
        }
    }
}
