using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Camp_FourthWeek_Basic_C__
{
    #region[Enum]
    public enum JobType
    {
        None,
        Warrior,
        Assassin
    }
    public enum StatType
    {
        Attack,
        Defense,
        MaxHP,
        CurHP,
        MaxMP,
        CurMP,

        CriticalChance,
    }
    public enum Menu
    {
        Menu,
        ShowInfo,
        Inventory,
        Shop,
        Dungeon,
        Rest
    }
    public enum ItemType
    {
        Weapon,
        Helmet,
        Armor,
        Groves,
        Shoes,

    }
    #endregion[Enum]


    public interface IAction
    {
        string Name { get; }

        void Excute();
    }

    public class PlayerInfo
    {
        public string Name { get; private set; }
        public Job Job { get; private set; }
        public Dictionary<StatType, Stat> Stats { get; private set; } = new Dictionary<StatType, Stat>();

        public int Gold = 1500;
        public PlayerInfo(JobType _job, string _name)
        {
            Job = JobTable.JobDataDic[_job];
            Stats = Job.Stats.ToDictionary();
            Name = _name;
        }
    }

    public class Item
    {
        public int Key {  get; private set; }
        public string Name = string.Empty;
        public ItemType ItemType;
        [JsonProperty]
        public bool IsEquipment => EquipmentManager.IsEquipped(this);
        public List<Stat> Stats = new List<Stat>();

        public int Cost;
        public string Descript = string.Empty;

        public Item(int _key,string _name, ItemType _type,List<Stat> _stats, string _descript, int _cost)
        {
            Key = _key;
            Name = _name;
            ItemType = _type;
            Stats = _stats;
            Descript = _descript;
            Cost = _cost;
        }
    }
    public class Stat
    {
        public StatType Type;
        public float BaseValue = 0;
        public float EquipmentValue = 0;
        public float BuffValue = 0;
        public float FinalValue => BaseValue + EquipmentValue + BuffValue;

        public Stat() { }
        public Stat(StatType _type)
        {
            Type = _type;
        }
        public Stat(StatType _type, int _value)
        {
            Type = _type;
            BaseValue = _value;
        }
        public void ModifyBaseValue(float _value, float _min = 0, float _max = int.MaxValue)
        {
            BaseValue += _value;
            BaseValue = Math.Clamp(BaseValue, _min, _max);
        }
        public void ModifyEquipmentValue(float _value)
        {
            EquipmentValue += _value;
            EquipmentValue = Math.Clamp(EquipmentValue, 0, int.MaxValue);
        }
        public void ModifyAllValue(float _value, float _min = 0, float _max = int.MaxValue)
        {
            float remainingDam = _value;
            if (remainingDam > 0)
            {
                float damToEquip = Math.Min(remainingDam, EquipmentValue);
                ModifyEquipmentValue(-damToEquip);
                remainingDam -= damToEquip;
            }
            if (remainingDam > 0)
            {
                ModifyBaseValue(-remainingDam, 0, FinalValue);
            }
        }
        public string GetStatName()
        {
            switch (Type)
            {
                case StatType.Attack:
                    return "공격력";
                case StatType.Defense:
                    return "방어력";
                case StatType.MaxHP:
                    return "최대 HP";
                case StatType.CurHP:
                    return "HP";
                case StatType.MaxMP:
                    return "최대 MP";
                case StatType.CurMP:
                    return "MP";
                case StatType.CriticalChance:
                    return "치명타확률";

                default: return string.Empty;
            }
        }


    }
    public class Dungeon
    {
        //권장 방어력
        PlayerInfo playerInfo = GameManager.PlayerInfo;
        public string DungeonName { get; private set; }
        public Stat RecommendedStat { get; private set; }
        public int RewardGold { get; private set; }

        public Dungeon(string _dungeonName,Stat _recommendedStat, int _rewardGold)
        {
            DungeonName = _dungeonName;
            RecommendedStat = _recommendedStat;
            RewardGold = _rewardGold;
        }

        public string ClearDungeon()
        {
            LevelManager.AddClearCount();
            Random rand = new Random();
            float stat = playerInfo.Stats[RecommendedStat.Type].FinalValue;
            Stat curHP = playerInfo.Stats[StatType.CurHP];
            RewardGold += rand.Next((int)stat, (int)(stat * 2 + 1));
            float damage = rand.Next(20, 36);

            damage -= stat - RecommendedStat.FinalValue;


            float originHP = curHP.FinalValue;
            curHP.ModifyAllValue(damage);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("던전 클리어");
            sb.AppendLine("축하 합니다!!");
            sb.AppendLine($"{DungeonName}을 클리어 하였습니다.");

            sb.AppendLine("[탐험 결과]");
            sb.AppendLine($"체력 {originHP} -> {curHP.FinalValue}");
            sb.AppendLine($"Gold {playerInfo.Gold} -> {playerInfo.Gold + RewardGold}");

            playerInfo.Gold += RewardGold;

            return sb.ToString();
        }
        public string UnClearDungeon()
        {
            Random rand = new Random();

            int damage = rand.Next(20, 36) / 2;
            Stat curHP = playerInfo.Stats[StatType.CurHP];
            float originHP = curHP.FinalValue;

            curHP.ModifyAllValue(damage);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("던전 공략 실패");
            sb.AppendLine($"{DungeonName} 공략에 실패 하였습니다.");

            sb.AppendLine("[탐험 결과]");
            sb.AppendLine($"체력 {originHP} -> {curHP.FinalValue}");

            return sb.ToString();
        }
    }

    public class Job
    {
        public JobType Type { get; private set; }
        public string Name { get; private set; }
        public Dictionary<StatType, Stat> Stats { get; private set; }

        public Job(JobType _type,string _name, Dictionary<StatType,Stat> _stat)
        {
            Type = _type;
            Name = _name;
            Stats = _stat;
        }
    }

    public class SaveData
    {
        public string Name;
        public JobType Job;
        public int Gold;
        public float Health;

        // Item을 전부 변환 시킬 필요가 없음. int값만 가지고 와서 Table에서 가져오는 방식을 사용
        public List<int> Inventory;
        public List<int> EquipmentItem;
        public int DungeonClearCount;
        public SaveData(SaveData _data)
        {
            Name= _data.Name;
            Job = _data.Job;
            DungeonClearCount = _data.DungeonClearCount;
            Inventory = _data.Inventory;
            EquipmentItem = _data.EquipmentItem;
            Gold = _data.Gold;
            Health = _data.Health;
        }
        public SaveData() { }
    }


}
