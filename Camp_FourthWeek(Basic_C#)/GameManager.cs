using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Camp_FourthWeek_Basic_C__
{
    internal static class GameManager
    {
        const string path = "saveData.json";
        public static PlayerInfo? PlayerInfo { get; private set; }

        public static SaveData loadData;

        public static void Init(JobType _job, string _name)
        {
            PlayerInfo = new PlayerInfo(_job, _name); //스탯 세팅이 되고

            if(loadData != null)
            {
                for (int i = 0; i < loadData.Inventory.Count; i++)
                {
                    Item item = ItemTable.GetItemByID(loadData.Inventory[i]);
                    InventoryManager.AddItem(item);
                    if(loadData.EquipmentItem.Contains(item.Key))
                    {
                        EquipmentManager.EquipmentItem(item);
                    }
                }
                for (int i = 0; i < loadData.DungeonClearCount; i++)
                {
                    LevelManager.AddClearCount();
                }
                PlayerInfo.Gold = loadData.Gold;
            }
        }


        public static void SaveGame()
        {
            List<int> inventory = InventoryManager.Inventory.Select(x => x.Key).ToList();
            List<int> equipmentItem = EquipmentManager.EquipmentItems.Values.Select(x => x.Key).ToList();


            SaveData saveData = new SaveData()
            {
                Name = PlayerInfo.Name,
                Job = PlayerInfo.Job.Type,
                Gold = PlayerInfo.Gold,

                Inventory = inventory,
                EquipmentItem = equipmentItem,
                DungeonClearCount = LevelManager.ClearDungeonCount,
            };
            string sJson = JsonConvert.SerializeObject(saveData, Formatting.Indented);



            File.WriteAllText(path, sJson);
        }
        public static void LoadGame()
        {
            if (!File.Exists(path))
            {
                var start = new CreateCharacterAction();
                start.Excute();
                return;
            }
            string json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<SaveData>(json);
            if (data != null)
            {
                loadData = new SaveData(data);
                Init(loadData.Job, loadData.Name);
                var mainAction = new MainMenuAction();
                mainAction.InitializeMainActions(mainAction);
                mainAction.Excute();
            }

        }
    }
}
