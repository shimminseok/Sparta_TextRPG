using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camp_FourthWeek_Basic_C__
{
    internal static class EquipmentManager
    {
        public static void EquipmentItem(Item _equipItem)
        {
            PlayerInfo player = GameManager.PlayerInfo;
            if(player.EquipmentItems.TryGetValue(_equipItem.ItemType, out var item))
            {
                //장착한 아이템이 있다면
                UnequipItem(item.ItemType);
            }
            _equipItem.IsEquipment = true;

            for (int i = 0; i < _equipItem.Stats.Count; i++)
            {
                Stat stat = _equipItem.Stats[i];
                player?.Stats[stat.Type].ModifyEquipmentValue(stat.FinalValue);
            }
            player.EquipmentItems[_equipItem.ItemType] = _equipItem;
        }

        public static void UnequipItem(ItemType _type)
        {
            PlayerInfo player = GameManager.PlayerInfo;
            Item equipItem = player.EquipmentItems[_type];
            if (equipItem != null)
            {
                for (int i = 0; i < equipItem.Stats.Count; i++)
                {
                    player.Stats[equipItem.Stats[i].Type].ModifyEquipmentValue(-equipItem.Stats[i].FinalValue);
                }
                equipItem.IsEquipment = false;
            }    
        }
    }
}
