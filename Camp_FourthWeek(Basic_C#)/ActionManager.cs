using System.Text;
using static Camp_FourthWeek_Basic_C__.StringUtil;

namespace Camp_FourthWeek_Basic_C__;

/// <summary>
///     모든 Action의 부모 클래스이며 모든 Action은 해당 메서드를 참조받아야함.
/// </summary>
public abstract class ActionBase : IAction
{
    private string feedBackMessage = string.Empty;
    protected IAction? PrevAction;
    protected Dictionary<int, IAction> SubActionMap = new();
    protected PlayerInfo PlayerInfo { get; } = GameManager.Instance.PlayerInfo;
    public abstract string Name { get; }

    public void Execute()
    {
        Console.Clear();
        Console.WriteLine($"[{Name}]");
        OnExcute();
    }

    public void SetFeedBackMessage(string _message)
    {
        feedBackMessage = _message;
    }

    public abstract void OnExcute();

    public void SelectAndRunAction(Dictionary<int, IAction> _actionMap)
    {
        Console.WriteLine();
        foreach (var action in _actionMap) Console.WriteLine($"{action.Key}. {action.Value.Name}");
        Console.WriteLine();
        Console.WriteLine($"0. {(PrevAction == null ? "종료하기" : $"{PrevAction.Name}로 되돌아가기")}");
        Console.WriteLine();
        Console.WriteLine(feedBackMessage);
        Console.WriteLine("원하시는 행동을 입력해주세요.");
        while (true)
            if (int.TryParse(Console.ReadLine(), out var id))
            {
                if (id == 0)
                {
                    if (PrevAction == null && this is MainMenuAction)
                        GameManager.Instance.SaveGame();
                    else
                        PrevAction?.Execute();
                    break;
                }

                if (_actionMap.ContainsKey(id))
                {
                    _actionMap[id].Execute();
                    break;
                }
                //개발자의 이스터에그
                else if (id == 527)
                {
                    var miniGame = new MiniGame();
                    miniGame.StartGame();
                    break;
                }

                Console.WriteLine("잘못된 입력입니다.");
            }
            else
            {
                Console.WriteLine("잘못된 입력입니다.");
            }


        feedBackMessage = string.Empty;
    }
}

public class CreateCharacterAction : ActionBase
{
    public CreateCharacterAction()
    {
        SubActionMap = new Dictionary<int, IAction>
        {
            { 1, new CreateNickNameAction() }
        };
    }

    public override string Name => "캐릭터 생성";

    public override void OnExcute()
    {
        Console.WriteLine("스파르타마을에 오신 모험가님을 환영합니다.");
        SelectAndRunAction(SubActionMap);
    }
}

public class CreateNickNameAction : ActionBase
{
    private string? nickName = string.Empty;
    public override string Name => "닉네임 설정";

    public override void OnExcute()
    {
        SubActionMap.Clear();
        do
        {
            Console.WriteLine("모험가님의 이름을 설정해주세요.");
            nickName = Console.ReadLine();
        } while (string.IsNullOrEmpty(nickName));

        Console.WriteLine($"{nickName}님 안녕하세요.");
        SubActionMap = new Dictionary<int, IAction>
        {
            { 1, new SelectedJobAction(nickName) }
        };
        SelectAndRunAction(SubActionMap);
    }
}

public class SelectedJobAction : ActionBase
{
    public SelectedJobAction(string _name)
    {
        foreach (var job in JobTable.JobDataDic.Values)
            SubActionMap.Add((int)job.Type, new SelectJobAction(job, _name));
    }

    public override string Name => "직업 선택";

    public override void OnExcute()
    {
        Console.WriteLine("플레이 하실 직업을 선택해주세요.");
        Console.WriteLine();
        foreach (var job in JobTable.JobDataDic.Values) Console.Write($"\t{job.Name}");
        Console.WriteLine();
        SelectAndRunAction(SubActionMap);
    }
}

public class SelectJobAction : ActionBase
{
    public SelectJobAction(Job _job, string _name)
    {
        Job = _job;
        CharacterName = _name;
    }

    public override string Name => Job.Name;
    public Job Job { get; }

    public string CharacterName { get; }

    public override void OnExcute()
    {
        Console.WriteLine($"직업 : {Job.Name}을 선택하셨습니다.");

        Console.WriteLine("잠시 후 게임이 시작됩니다.");
        GameManager.Instance.Init(Job.Type, CharacterName);
        var main = new MainMenuAction();
        Thread.Sleep(1000);
        main.Execute();
    }
}

public class MainMenuAction : ActionBase
{
    public MainMenuAction()
    {
        InitializeMainActions(this);
    }

    public override string Name => "마을";

    public void InitializeMainActions(MainMenuAction mainAction)
    {
        SubActionMap[1] = new EnterCharacterInfoAction(mainAction);
        SubActionMap[2] = new EnterInventoryAction(mainAction);
        SubActionMap[3] = new EnterShopAction(mainAction);
        SubActionMap[4] = new EnterDungeonAction(mainAction);
        SubActionMap[5] = new EnterRestAction(mainAction);
        SubActionMap[6] = new EnterResetAction(mainAction);

        SubActionMap[99] = new TestAction(mainAction);
    }

    public override void OnExcute()
    {
        Console.WriteLine("스파르타 마을에 오신 여러분 환영합니다.");
        Console.WriteLine("이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.");

        SelectAndRunAction(SubActionMap);
    }
}

public class EnterCharacterInfoAction : ActionBase
{
    public EnterCharacterInfoAction(IAction _prevAction)
    {
        PrevAction = _prevAction;
    }

    public override string Name => "상태 보기";

    public override void OnExcute()
    {
        Console.WriteLine("캐릭터의 정보가 표시됩니다.");
        Console.WriteLine();

        Console.WriteLine($"{PadRightWithKorean("Lv.", 12)} : {LevelManager.CurrentLevel}");
        Console.WriteLine($"{PadRightWithKorean("닉네임 : ", 12)} : {PlayerInfo.Name}");
        Console.WriteLine(PadRightWithKorean($"{PadRightWithKorean("직업", 12)} : {PlayerInfo.Job.Name}", 10));
        foreach (var stat in PlayerInfo.Stats.Values)
            Console.WriteLine(
                $"{PadRightWithKorean(stat.GetStatName(), 12)} : {PadRightWithKorean(stat.FinalValue.ToString("N0"), 9)} +({stat.EquipmentValue})");
        Console.WriteLine($"{PadRightWithKorean("Gold", 12)} : {PlayerInfo.Gold}");
        Console.WriteLine();

        SelectAndRunAction(SubActionMap);
    }
}

public class EnterShopAction : ActionBase
{
    public List<Item> SaleItems = new();

    public EnterShopAction(IAction _prevAction)
    {
        PrevAction = _prevAction;
        SubActionMap = new Dictionary<int, IAction>
        {
            { 1, new BuyItemAction(this) },
            { 2, new SellItemAction(this) }
        };

        SaleItems = ItemTable.ItemDic.Values.ToList();
    }

    public override string Name => "상점";

    public override void OnExcute()
    {
        Console.WriteLine("필요한 아이템을 얻을 수 있는 상점입니다.");
        Console.WriteLine();
        ShowSaleItems();
        SelectAndRunAction(SubActionMap);
    }

    private void ShowSaleItems()
    {
        Console.WriteLine("[보유 골드]");
        Console.WriteLine($"{PlayerInfo.Gold}G");
        Console.WriteLine();
        Console.WriteLine("[아이템 목록]");
        for (var i = 0; i < SaleItems.Count; i++)
        {
            var item = SaleItems[i];
            var sb = UiManager.ItemPrinter(item, i);
            sb.Append(" | ");
            if (InventoryManager.Instance.Inventory.Exists(x => x.Name == item.Name))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                sb.Append($"{PadRightWithKorean("구매완료", 10)}");
            }
            else
            {
                sb.Append($"{PadRightWithKorean($"{item.Cost}G", 10)}");
            }

            sb.Append(" | ");
            Console.WriteLine(sb.ToString());
            Console.ResetColor();
        }
    }
}

public class BuyItemAction : ActionBase
{
    private List<Item> SaleItems = new();

    public BuyItemAction(IAction _prevAction)
    {
        PrevAction = _prevAction;
    }

    public override string Name => "상점 - 아이템 구매";

    public override void OnExcute()
    {
        SaleItems = ItemTable.ItemDic.Values
            .Where(item => !InventoryManager.Instance.Inventory.Any(x => x.Key == item.Key))
            .ToList();
        SubActionMap.Clear();
        for (var i = 0; i < SaleItems.Count; i++)
            if (SubActionMap.ContainsKey(i + 1))
                SubActionMap[i + 1] = new BuyAction(SaleItems[i], this);
            else
                SubActionMap.Add(i + 1, new BuyAction(SaleItems[i], this));

        Console.WriteLine("필요한 아이템을 구매할 수 있습니다.");
        Console.WriteLine("[보유 골드]");
        Console.WriteLine($"{PlayerInfo.Gold}G");
        Console.WriteLine("[아이템 목록]");
        ShowItemInfo();


        Console.WriteLine();
        SelectAndRunAction(SubActionMap);
    }

    private void ShowItemInfo()
    {
        for (var i = 0; i < SaleItems.Count; i++)
        {
            var item = SaleItems[i];
            var sb = UiManager.ItemPrinter(item, i);
            if (InventoryManager.Instance.Inventory.Exists(x => x.Name == item.Name))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                sb.Append($"{PadRightWithKorean("구매완료", 10)}");
            }
            else
            {
                sb.Append($"{PadRightWithKorean($"{item.Cost}G", 10)}");
            }

            sb.Append(" | ");
            Console.WriteLine(sb.ToString());
            Console.ResetColor();
        }
    }
}

public class BuyAction : ActionBase
{
    private readonly Item item;

    public BuyAction(Item _item, IAction _prevAction)
    {
        item = _item;
        PrevAction = _prevAction;
    }

    public override string Name => $"{item.Name} 구매";

    public override void OnExcute()
    {
        var message = string.Empty;
        if (PlayerInfo != null)
        {
            if (PlayerInfo.Gold < item.Cost)
            {
                message = "골드가 부족합니다.";
            }
            else
            {
                PlayerInfo.Gold -= item.Cost;
                InventoryManager.Instance.Inventory.Add(item);
                message = $"{item.Name}을(를) 구매했습니다!";
            }
        }

        if (PrevAction != null)
        {
            PrevAction.SetFeedBackMessage(message);
            PrevAction.Execute();
        }
    }
}

public class SellItemAction : ActionBase
{
    public SellItemAction(IAction _prevAction)
    {
        PrevAction = _prevAction;
    }

    public override string Name => "상점 - 아이템 판매";

    public override void OnExcute()
    {
        SubActionMap.Clear();
        for (var i = 0; i < InventoryManager.Instance.Inventory.Count; i++)
            SubActionMap.Add(i + 1, new SellAction(InventoryManager.Instance.Inventory[i], this));
        ShowItemInfo();
        Console.WriteLine();
        Console.WriteLine();
        SelectAndRunAction(SubActionMap);
    }

    private void ShowItemInfo()
    {
        for (var i = 0; i < InventoryManager.Instance.Inventory.Count; i++)
        {
            var item = InventoryManager.Instance.Inventory[i];
            var sb = UiManager.ItemPrinter(item, i, false);
            sb.Append($"| {PadRightWithKorean($"{item.Cost * 0.85}G", 5)}");

            Console.WriteLine(sb.ToString());
            Console.ResetColor();
        }
    }
}

public class SellAction : ActionBase
{
    private readonly Item item;

    public SellAction(Item _item, IAction _prevAction)
    {
        PrevAction = _prevAction;
        item = _item;
    }

    public override string Name => $"{item.Name} 판매하기";

    public override void OnExcute()
    {
        PlayerInfo.Gold += (int)(item.Cost * 0.85);
        InventoryManager.Instance.RemoveItem(item);

        PrevAction!.SetFeedBackMessage($"{item.Name}을 판매했습니다. (보유골드 : {PlayerInfo.Gold})");
        PrevAction.Execute();
    }
}

public class EnterInventoryAction : ActionBase
{
    public EnterInventoryAction(IAction _prevAction)
    {
        PrevAction = _prevAction;
        SubActionMap = new Dictionary<int, IAction>
        {
            { 1, new EquipItemManagementAction(this) }
        };
    }

    public override string Name => "인벤토리";

    public override void OnExcute()
    {
        Console.WriteLine("보유 중인 아이템을 관리할 수 있습니다.");

        Console.WriteLine("[아이템 목록]");
        for (var i = 0; i < InventoryManager.Instance.Inventory.Count; i++)
        {
            var item = InventoryManager.Instance.Inventory[i];
            var sb = UiManager.ItemPrinter(item, i);

            if (item.IsEquipment)
            {
                var idx = sb.ToString().IndexOf(item.Name);
                Console.ForegroundColor = ConsoleColor.Green;
                sb.Insert(idx, "[E]");
            }

            Console.WriteLine(sb.ToString());
            Console.ResetColor();
        }

        SelectAndRunAction(SubActionMap);
    }
}

/// <summary>
///     장착을 관리하는 Action
/// </summary>
public class EquipItemManagementAction : ActionBase
{
    public EquipItemManagementAction(IAction _prevAction)
    {
        PrevAction = _prevAction;
    }

    public override string Name => "인벤토리 - 장착 관리";

    public override void OnExcute()
    {
        SubActionMap.Clear();
        Console.WriteLine("보유 중인 아이템을 장착할 수 있습니다.");
        Console.WriteLine("[아이템 목록]");
        for (var i = 0; i < InventoryManager.Instance.Inventory.Count; i++)
        {
            var item = InventoryManager.Instance.Inventory[i];
            var sb = new StringBuilder();
            sb.Append($"{PadRightWithKorean($"- {i + 1}", 5)}");
            if (item.IsEquipment)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                sb.Append("[E]");
            }

            sb.Append($"{PadRightWithKorean($"{item.Name}", 18)}");
            for (var j = 0; j < item.Stats.Count; j++)
                sb.Append(
                    $" | {PadRightWithKorean($"{item.Stats[j].GetStatName()} +{item.Stats[j].FinalValue}", 10)} ");
            sb.Append(" | ");
            sb.Append($"{PadRightWithKorean($"{item.Description}", 50)}");


            Console.WriteLine(sb.ToString());
            Console.ResetColor();

            if (!SubActionMap.ContainsKey(i + 1))
                SubActionMap[i + 1] = new EquipAction(InventoryManager.Instance.Inventory[i], this);
        }

        Console.WriteLine();
        SelectAndRunAction(SubActionMap);
    }
}

/// <summary>
///     아이템을 장착하는 Action
/// </summary>
public class EquipAction : ActionBase
{
    private readonly Item item;

    public EquipAction(Item _item, IAction _prevAction)
    {
        item = _item;
        PrevAction = _prevAction;
    }

    public override string Name => $"{item.Name} 아이템 {(item.IsEquipment ? "해제" : "장착")}";

    public override void OnExcute()
    {
        var message = string.Empty;
        if (item.IsEquipment)
        {
            message = $"{item.Name}이 장착 해제 되었습니다.";
            EquipmentManager.UnequipItem(item.ItemType);
        }
        else
        {
            message = $"{item.Name}이 장착 되었습니다.";
            EquipmentManager.EquipmentItem(item);
        }

        Console.WriteLine(message);
        PrevAction?.Execute();
    }
}

public class EnterRestAction : ActionBase
{
    public EnterRestAction(IAction _prevAction)
    {
        PrevAction = _prevAction;
        SubActionMap = new Dictionary<int, IAction>
        {
            { 1, new RecoverAction(this) }
        };
    }

    public override string Name => "휴식 하기";

    public override void OnExcute()
    {
        Console.WriteLine($"500G를 내면 체력을 회복할 수 있습니다. (보유 골드 : {PlayerInfo.Gold})");
        SelectAndRunAction(SubActionMap);
    }
}

public class RecoverAction : ActionBase
{
    public RecoverAction(IAction _prevAction)
    {
        PrevAction = _prevAction;
    }

    public override string Name => "휴식하기";

    public override void OnExcute()
    {
        var message = string.Empty;
        if (PlayerInfo.Gold < 500)
        {
            message = "골드가 부족합니다.";
        }
        else
        {
            var curHP = PlayerInfo.Stats[StatType.CurHp];
            var before = curHP.FinalValue;
            curHP.ModifyBaseValue(PlayerInfo.Stats[StatType.MaxHp].FinalValue, 0,
                PlayerInfo.Stats[StatType.MaxHp].FinalValue);
            message = $"체력이 회복되었습니다 HP {before} -> {curHP.FinalValue}";
        }

        PrevAction!.SetFeedBackMessage(message);
        PrevAction.Execute();
    }
}

public class EnterDungeonAction : ActionBase
{
    public EnterDungeonAction(IAction _prevAction)
    {
        PrevAction = _prevAction;
        SubActionMap = new Dictionary<int, IAction>();
        for (var i = 0; i < DungeonTable.DungeonDic.Count; i++) SubActionMap.Add(i + 1, new DungeonAction(i + 1, this));
    }

    public override string Name => "던전 입장";

    public override void OnExcute()
    {
        Console.WriteLine("");

        Console.WriteLine("[던전 목록]");
        Console.WriteLine($"현재 공격력 : {PlayerInfo.Stats[StatType.Attack].FinalValue}");
        Console.WriteLine($"현재 방어력 : {PlayerInfo.Stats[StatType.Defense].FinalValue}");

        SelectAndRunAction(SubActionMap);
    }
}

public class DungeonAction : ActionBase
{
    private readonly Dungeon? dungeon;

    public DungeonAction(int _dunNum, IAction _prevAction)
    {
        dungeon = DungeonTable.GetDungeonById(_dunNum);
        PrevAction = _prevAction;
    }

    public override string Name => SetName();

    public override void OnExcute()
    {
        //던전 도전 조건 판단
        var dungeonStat = dungeon.RecommendedStat;
        var playerStat = PlayerInfo.Stats[dungeonStat.Type].FinalValue;

        if (35 - playerStat - dungeonStat.FinalValue > PlayerInfo.Stats[StatType.CurHp].FinalValue)
        {
            PrevAction!.SetFeedBackMessage("체력이 부족합니다.");
            PrevAction?.Execute();
            return;
        }

        var rans = new Random();
        float damage = rans.Next(20, 36);
        damage -= playerStat - dungeonStat.FinalValue;


        string message;
        var sb = new StringBuilder();

        //권장 방어력이 높으면
        if (dungeonStat.FinalValue > playerStat)
        {
            //던전을 실패할 수있음 40프로 확률
            var percent = rans.NextSingle();
            if (percent <= 0.4f)
                message = dungeon.UnClearDungeon(damage);
            else
                message = dungeon.ClearDungeon(damage);
        }
        else
        {
            message = dungeon.ClearDungeon(damage);
        }


        PrevAction!.SetFeedBackMessage(message);
        PrevAction!.Execute();
    }

    private string SetName()
    {
        var sb = new StringBuilder();
        sb.Append($"{PadRightWithKorean($"{dungeon.DungeonName}", 12)} ");

        sb.Append(PadRightWithKorean($"| {dungeon.RecommendedStat.GetStatName()}", 10));
        sb.Append(PadRightWithKorean($"+{dungeon.RecommendedStat.FinalValue} 이상", 10));

        sb.Append(" |");
        return sb.ToString();
    }
}

public class EnterResetAction : ActionBase
{
    public EnterResetAction(IAction _action)
    {
        PrevAction = _action;
    }

    public override string Name => "리셋!!!";

    public override void OnExcute()
    {
        Console.WriteLine("저장된 데이터를 삭제하시겠습니까?");
        Console.WriteLine("1. 예 \t 2.아니오");

        if (int.TryParse(Console.ReadLine(), out var input))
        {
            if (input == 1)
                GameManager.Instance.DeleteGameData();
            else if (input == 2) PrevAction?.Execute();
        }
    }
}

public class TestAction : ActionBase
{
    public TestAction(IAction _prevAction)
    {
        PrevAction = _prevAction;
    }

    public override string Name => "이건 테스트에오";

    public override void OnExcute()
    {
        var sb = new StringBuilder();
        sb.AppendLine("테스트 액션 함수 입니다.");
        sb.AppendLine("하위 액션이 없으면 상위 액션으로 돌아가도록 설정했습니다.");


        PrevAction.SetFeedBackMessage(sb.ToString());
        PrevAction?.Execute();
    }
}