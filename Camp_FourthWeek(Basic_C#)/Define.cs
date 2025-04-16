using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Camp_FourthWeek_Basic_C__
{
    #region[Enum]
    public enum JobType
    {
        Warrior,
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
    public interface IHasSubActions
    {
        IAction GetSubAction(int _key);
    }

    public class PlayerInfo
    {
        public int Level = 1;
        public int Gold = 1500;
        public JobType Job;
        public JobBase JobBase;

        public Dictionary<StatType, Stat> Stats = new Dictionary<StatType, Stat>();
        public Dictionary<ItemType, Item> EquipmentItems = new Dictionary<ItemType, Item>();

        //public List<Skill> SkillList = new List<Skill>();

        public PlayerInfo(JobType _job)
        {
            switch (_job)
            {
                case JobType.Warrior:
                    JobBase = new Warrior(_job);
                    break;
            }
            Stats = JobBase.Stats.ToDictionary();
        }
    }

    public class Item
    {
        public string Name = string.Empty;
        public ItemType ItemType;
        public bool IsEquipment = false;
        public List<Stat> Stats = new List<Stat>();

        public int Cost;
        public string Descript = string.Empty;

        public Item(string _name, ItemType _type,List<Stat> _stats, string _descript, int _cost)
        {
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
        public int BaseValue = 0;
        public int EquipmentValue = 0;
        public int BuffValue = 0;
        public int FinalValue => BaseValue + EquipmentValue + BuffValue;

        public Stat(StatType _type)
        {
            Type = _type;
        }
        public Stat(StatType _type, int _value)
        {
            Type = _type;
            BaseValue = _value;
        }
        public void ModifyBaseValue(int _value, int _min = 0, int _max = int.MaxValue)
        {
            BaseValue += _value;
            BaseValue = Math.Clamp(BaseValue, _min, _max);
        }
        public void ModifyEquipmentValue(int _value)
        {
            EquipmentValue += _value;
            EquipmentValue = Math.Clamp(EquipmentValue, 0, int.MaxValue);
        }
        public void ModifyAllValue(int _value, int _min = 0, int _max = int.MaxValue)
        {
            int remainingDam = _value;
            if (remainingDam > 0)
            {
                int damToEquip = (int)Math.Min(remainingDam, EquipmentValue);
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
            Random rand = new Random();

            int stat = playerInfo.Stats[RecommendedStat.Type].FinalValue;
            Stat curHP = playerInfo.Stats[StatType.CurHP];
            RewardGold += rand.Next(stat, (stat * 2 + 1));
            int damage = rand.Next(20, 36);

            damage -= stat - RecommendedStat.FinalValue;


            int originHP = curHP.FinalValue;
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
            int originHP = curHP.FinalValue;

            curHP.ModifyAllValue(damage);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("던전 공략 실패");
            sb.AppendLine($"{DungeonName} 공략에 실패 하였습니다.");

            sb.AppendLine("[탐험 결과]");
            sb.AppendLine($"체력 {originHP} -> {curHP.FinalValue}");

            return sb.ToString();
        }
    }

    public class JobBase
    {
        public JobType Type { get; protected set; }
        public string Name { get; protected set; }
        public Dictionary<StatType, Stat> Stats { get; protected set; }


    }


    public class Warrior : JobBase
    {
        public Warrior(JobType type)
        {
            Type = type;
            Name = "전사";
            Stats = new Dictionary<StatType, Stat>
            {
                {StatType.Attack, new Stat(StatType.Attack,10) },
                {StatType.Defense, new Stat(StatType.Defense,5) },
                {StatType.MaxHP, new Stat(StatType.MaxHP,100) },
                {StatType.CurHP, new Stat(StatType.CurHP,100) },
                {StatType.MaxMP, new Stat(StatType.MaxMP,100) },
                {StatType.CurMP, new Stat(StatType.CurMP,100) },
                {StatType.CriticalChance, new Stat(StatType.CriticalChance,3) },

            };
        }
    }


}
