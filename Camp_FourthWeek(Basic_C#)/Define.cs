using System.Text;

namespace Camp_FourthWeek_Basic_C__;

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
    MaxHp,
    CurHp,
    MaxMp,
    CurMp,

    CriticalChance
}

public enum ItemType
{
    Weapon,
    Helmet,
    Armor,
    Groves,
    Shoes
}

#endregion[Enum]

public interface IAction
{
    string Name { get; }

    void Execute();
    void SetFeedBackMessage(string _message);
}

public class PlayerInfo
{
    public int Gold = 1500;

    public PlayerInfo(JobType _job, string _name)
    {
        Job = JobTable.JobDataDic[_job];
        Stats = Job.Stats.ToDictionary();
        Name = _name;
    }

    public string Name { get; private set; }
    public Job Job { get; }
    public Dictionary<StatType, Stat> Stats { get; }
}

public class Item(int _key, string _name, ItemType _type, List<Stat> _stats, string _description, int _cost)
{
    public readonly int Cost = _cost;
    public readonly string Description = _description;
    public readonly ItemType ItemType = _type;
    public readonly string Name = _name;
    public readonly List<Stat> Stats = _stats;
    public int Key { get; private set; } = _key;


    public bool IsEquipment => EquipmentManager.IsEquipped(this);
}

public class Stat
{
    public float BaseValue;
    public float BuffValue = 0;
    public float EquipmentValue;
    public StatType Type;

    public Stat()
    {
    }

    public Stat(StatType _type, int _value)
    {
        Type = _type;
        BaseValue = _value;
    }

    public float FinalValue => BaseValue + EquipmentValue + BuffValue;

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
        if (_value <= 0)
            _value = 1;

        var remainingDam = _value;
        if (remainingDam > 0)
        {
            var damToEquip = Math.Min(remainingDam, EquipmentValue);
            ModifyEquipmentValue(-damToEquip);
            remainingDam -= damToEquip;
        }

        if (remainingDam > 0) ModifyBaseValue(-remainingDam, 0, FinalValue);
    }

    public string GetStatName()
    {
        switch (Type)
        {
            case StatType.Attack:
                return "공격력";
            case StatType.Defense:
                return "방어력";
            case StatType.MaxHp:
                return "최대 HP";
            case StatType.CurHp:
                return "HP";
            case StatType.MaxMp:
                return "최대 MP";
            case StatType.CurMp:
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
    private readonly PlayerInfo playerInfo = GameManager.Instance.PlayerInfo;

    public Dungeon(string _dungeonName, Stat _recommendedStat, int _rewardGold)
    {
        DungeonName = _dungeonName;
        RecommendedStat = _recommendedStat;
        RewardGold = _rewardGold;
    }

    public string DungeonName { get; }
    public Stat RecommendedStat { get; private set; }
    public int RewardGold { get; private set; }

    public string ClearDungeon(float dam)
    {
        LevelManager.AddClearCount();
        var rand = new Random();
        var stat = playerInfo.Stats[StatType.Attack].FinalValue;
        var curHP = playerInfo.Stats[StatType.CurHp];
        RewardGold += rand.Next((int)stat, (int)(stat * 2 + 1));

        var originHP = curHP.FinalValue;
        curHP.ModifyAllValue(dam);

        var sb = new StringBuilder();
        sb.AppendLine("던전 클리어");
        sb.AppendLine("축하 합니다!!");
        sb.AppendLine($"{DungeonName}을 클리어 하였습니다.");

        sb.AppendLine("[탐험 결과]");
        sb.AppendLine($"체력 {originHP} -> {curHP.FinalValue}");
        sb.AppendLine($"Gold {playerInfo.Gold} -> {playerInfo.Gold + RewardGold}");

        playerInfo.Gold += RewardGold;

        return sb.ToString();
    }

    public string UnClearDungeon(float _dam)
    {
        var rand = new Random();

        var damage = (int)(_dam / 2);
        var curHP = playerInfo.Stats[StatType.CurHp];
        var originHP = curHP.FinalValue;

        curHP.ModifyAllValue(damage);
        var sb = new StringBuilder();
        sb.AppendLine("던전 공략 실패");
        sb.AppendLine($"{DungeonName} 공략에 실패 하였습니다.");

        sb.AppendLine("[탐험 결과]");
        sb.AppendLine($"체력 {originHP} -> {curHP.FinalValue}");

        return sb.ToString();
    }
}

public class Job
{
    public Job(JobType _type, string _name, Dictionary<StatType, Stat> _stat)
    {
        Type = _type;
        Name = _name;
        Stats = _stat;
    }

    public JobType Type { get; private set; }
    public string Name { get; private set; }
    public Dictionary<StatType, Stat> Stats { get; }
}

public class SaveData
{
    public int DungeonClearCount;
    public List<int> EquipmentItem;
    public int Gold;

    // Item을 전부 변환 시킬 필요가 없음. int값만 가지고 와서 Table에서 가져오는 방식을 사용
    public List<int> Inventory;
    public JobType Job;
    public string Name;

    public SaveData(SaveData _data)
    {
        Name = _data.Name;
        Job = _data.Job;
        DungeonClearCount = _data.DungeonClearCount;
        Inventory = _data.Inventory;
        EquipmentItem = _data.EquipmentItem;
        Gold = _data.Gold;
    }

    public SaveData()
    {
    }
}