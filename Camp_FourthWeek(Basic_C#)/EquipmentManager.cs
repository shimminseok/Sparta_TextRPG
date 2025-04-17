using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camp_FourthWeek_Basic_C__
{
    internal static class EquipmentManager
    {
        public static Dictionary<ItemType, Item> EquipmentItems = new Dictionary<ItemType, Item>();

        public static void EquipmentItem(Item _equipItem)
        {
            PlayerInfo player = GameManager.Instance.PlayerInfo;
            if(EquipmentItems.TryGetValue(_equipItem.ItemType, out var item))
            {
                //장착한 아이템이 있다면
                UnequipItem(item.ItemType);
            }
            for (int i = 0; i < _equipItem.Stats.Count; i++)
            {
                Stat stat = _equipItem.Stats[i];
                player?.Stats[stat.Type].ModifyEquipmentValue(stat.FinalValue);
            }
            EquipmentItems[_equipItem.ItemType] = _equipItem;
        }

        public static void UnequipItem(ItemType _type)
        {
            PlayerInfo player = GameManager.Instance.PlayerInfo;
            Item equipItem = EquipmentItems[_type];
            if (equipItem != null)
            {
                for (int i = 0; i < equipItem.Stats.Count; i++)
                {
                    player.Stats[equipItem.Stats[i].Type].ModifyEquipmentValue(-equipItem.Stats[i].FinalValue);
                }
            }
            EquipmentItems.Remove(_type);
        }

        public static bool IsEquipped(Item _item) => EquipmentItems.ContainsValue(_item);
    }
}
