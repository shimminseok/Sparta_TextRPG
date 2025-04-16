using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Camp_FourthWeek_Basic_C__.Program;
using static StringUtil;


public static class StringUtil
{
    public static string PadRightWithKorean(string _str, int _totalWidth)
    {
        int width = 0;
        foreach (char c in _str)
        {
            width += IsKorean(c) ? 2 : 1;
        }

        int padding = _totalWidth - width;
        if (padding > 0)
        {
            return _str + new string(' ', padding);
        }

        return _str;
    }
    static bool IsKorean(char _c)
    {
        return (_c >= 0xAC00 && _c <= 0xD7A3);
    }
}

namespace Camp_FourthWeek_Basic_C__
{
    public abstract class ActionBase : IAction
    {
        public IAction? PrevAction { get; protected set; }
        public abstract string Name { get; }

        protected PlayerInfo playerInfo = GameManager.PlayerInfo;
        protected Dictionary<int, IAction> subActionMap = new Dictionary<int, IAction>();
        protected string FeedBackMessage = string.Empty;
        public abstract void OnExcute();

        public void SelectAndRunAction(Dictionary<int, IAction> _actionMap)
        {
            Console.WriteLine();
            foreach (var action in _actionMap)
            {
                Console.WriteLine($"{action.Key}. {action.Value.Name}");
            }
            Console.WriteLine();
            Console.WriteLine("0. 나가기");
            Console.WriteLine();
            Console.WriteLine(FeedBackMessage);
            Console.WriteLine("원하시는 행동을 입력해주세요.");
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out var id))
                {
                    if (id == 0)
                    {
                        PrevAction?.Excute();
                        break;
                    }
                    else if (_actionMap.ContainsKey(id))
                    {
                        _actionMap[id].Excute();
                        break;
                    }
                    else
                    {
                        Console.WriteLine("잘못된 입력입니다.");
                    }
                }
            }

        }
        public void Excute()
        {
            Console.WriteLine($"{Name}");
            OnExcute();
        }
    }
    public class MainMenuAction : ActionBase
    {
        public override string Name => "마을";

        public Dictionary<int, IAction> mainActions = new Dictionary<int, IAction>();
        public void InitializeMainActions(MainMenuAction mainAction)
        {
            mainActions.Clear();
            mainActions[(int)Menu.ShowInfo] = new EnterCharacterInfoAction(mainAction);
            mainActions[(int)Menu.Inventory] = new EnterInventoryAction(mainAction);
            mainActions[(int)Menu.Shop] = new EnterShopAction(mainAction);
            mainActions[(int)Menu.Dungeon] = new EnterDungeonAction(mainAction);
            mainActions[(int)Menu.Rest] = new EnterRestAction(mainAction);
        }

        public override void OnExcute()
        {
            Console.Clear();
            Console.WriteLine("스파르타 마을에 오신 여러분 환영합니다.");
            Console.WriteLine("이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.");

            SelectAndRunAction(mainActions);
        }
    }
    public class EnterCharacterInfoAction : ActionBase
    {
        public override string Name => "상태 보기";

        public EnterCharacterInfoAction(IAction _prevAction)
        {
            PrevAction = _prevAction;
        }
        public override void OnExcute()
        {
            Console.Clear();
            Console.WriteLine("캐릭터의 정보가 표시됩니다.");
            Console.WriteLine();

            Console.WriteLine($"Lv. {playerInfo.Level}");
            Console.WriteLine(PadRightWithKorean($"{PadRightWithKorean("직업", 9)} : {playerInfo.Job.ToString()}", 10));
            foreach (var stat in playerInfo.Stats.Values)
            {
                Console.WriteLine($"{PadRightWithKorean(stat.GetStatName(), 9)} : {PadRightWithKorean(stat.FinalValue.ToString("N0"), 9)} +({stat.EquipmentValue})");
            }
            Console.WriteLine($"{PadRightWithKorean("Gold", 9)} : {playerInfo.Gold}");
            Console.WriteLine();

            SelectAndRunAction(subActionMap);
        }
    }
    public class EnterShopAction : ActionBase, IHasSubActions
    {
        public override string Name => "상점";

        public List<Item> SaleItems = new List<Item>();

        public EnterShopAction(IAction _prevAction)
        {
            PrevAction = _prevAction;
            subActionMap = new Dictionary<int, IAction>()
                {
                    {1, new BuyItemAction(this) },
                    {2, new SellItemAction(this) },
                };

            SaleItems = ItemTable.itemDic.Values.ToList();
        }

        public override void OnExcute()
        {
            Console.WriteLine("필요한 아이템을 얻을 수 있는 상점입니다.");
            Console.WriteLine();
            ShowSaleItems();
            SelectAndRunAction(subActionMap);

        }

        void ShowSaleItems()
        {
            Console.Clear();
            Console.WriteLine("[보유 골드]");
            Console.WriteLine($"{playerInfo.Gold}G");
            Console.WriteLine();
            Console.WriteLine("[아이템 목록]");
            for (int i = 0; i < SaleItems.Count; i++)
            {
                Item item = SaleItems[i];
                StringBuilder sb = new StringBuilder();
                sb.Append($"{PadRightWithKorean($"- {i + 1}", 5)}");


                sb.Append($"{PadRightWithKorean($"{item.Name}", 18)}");
                for (int j = 0; j < item.Stats.Count; j++)
                {
                    sb.Append($" | {PadRightWithKorean($"{item.Stats[j].GetStatName()} +{item.Stats[j].FinalValue}", 10)} ");

                }
                sb.Append(PadRightWithKorean("", (2 / item.Stats.Count) * 10));
                sb.Append(" | ");
                sb.Append($"{PadRightWithKorean($"{item.Descript}", 50)}");

                if (InventoryManager.Inventory.Exists(x => x.Name == item.Name))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    sb.Append("| 구매완료");
                }
                else
                {
                    sb.Append($"| {PadRightWithKorean($"{item.Cost}G", 5)}");
                }
                sb.Append(" | ");
                Console.WriteLine(sb.ToString());
                Console.ResetColor();
            }
        }

        IAction IHasSubActions.GetSubAction(int _key)
        {
            return subActionMap[_key];
        }
    }
    public class BuyItemAction : ActionBase, IHasSubActions
    {
        public override string Name => "상점 - 아이템 구매";
        List<Item> SaleItems = new List<Item>();

        public BuyItemAction(IAction _prevAction)
        {
            PrevAction = _prevAction;
        }
        public override void OnExcute()
        {
            SaleItems = ItemTable.itemDic.Values.Where(item => !InventoryManager.Inventory.Any(x => x.Name == item.Name)).ToList();
            subActionMap.Clear();
            for (int i = 0; i < SaleItems.Count; i++)
            {
                if (subActionMap.ContainsKey(i + 1))
                {
                    subActionMap[i + 1] = new BuyAction(SaleItems[i], this);
                }
                else
                {
                    subActionMap.Add(i + 1, new BuyAction(SaleItems[i], this));
                }
            }

            Console.WriteLine("필요한 아이템을 구매할 수 있습니다.");
            Console.WriteLine();
            Console.WriteLine("[보유 골드]");
            Console.WriteLine($"{playerInfo.Gold}G");
            Console.WriteLine();
            Console.WriteLine("[아이템 목록]");
            ShowItemInfo();


            Console.WriteLine();
            SelectAndRunAction(subActionMap);
        }
        void ShowItemInfo()
        {
            Console.Clear();
            for (int i = 0; i < SaleItems.Count; i++)
            {
                Item item = SaleItems[i];
                StringBuilder sb = new StringBuilder();
                sb.Append($"{PadRightWithKorean($"- {i + 1}", 5)}");


                sb.Append($"{PadRightWithKorean($"{item.Name}", 18)}");
                for (int j = 0; j < item.Stats.Count; j++)
                {
                    sb.Append($" | {PadRightWithKorean($"{item.Stats[j].GetStatName()} +{item.Stats[j].FinalValue}", 10)} ");

                }
                sb.Append(" | ");
                sb.Append($"{PadRightWithKorean($"{item.Descript}", 50)}");

                if (InventoryManager.Inventory.Exists(x => x.Name == item.Name))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    sb.Append("구매완료");
                }
                else
                {
                    sb.Append($"| {PadRightWithKorean($"{item.Cost}G", 5)}");
                }
                Console.WriteLine(sb.ToString());
                Console.ResetColor();
            }
        }

        IAction IHasSubActions.GetSubAction(int _key)
        {
            return subActionMap[_key];
        }
        public void SetFeedBackMessage(string _msg)
        {
            FeedBackMessage = _msg;
        }
    }
    public class BuyAction : ActionBase
    {
        Item item;
        public override string Name => $"{item.Name} 구매";
        public BuyAction(Item _item, IAction _prevAction)
        {
            item = _item;
            PrevAction = _prevAction;
        }
        public override void OnExcute()
        {
            string message = string.Empty;
            if (playerInfo.Gold < item.Cost)
            {
                message = "골드가 부족합니다.";
            }
            else
            {
                playerInfo.Gold -= item.Cost;
                InventoryManager.Inventory.Add(item);
                message = $"{item.Name}을(를) 구매했습니다!";
            }

            ((BuyItemAction)PrevAction!).SetFeedBackMessage(message);
            PrevAction?.Excute();
        }
    }
    public class SellItemAction : ActionBase
    {
        public override string Name => "상점 - 아이템 판매";
        public SellItemAction(IAction _prevAction)
        {
            PrevAction = _prevAction;

        }
        public override void OnExcute()
        {
            subActionMap.Clear();
            for (int i = 0; i < InventoryManager.Inventory.Count; i++)
            {
                subActionMap.Add(i + 1, new SellAction(InventoryManager.Inventory[i], this));
            }
            ShowItemInfo();
            Console.WriteLine();
            Console.WriteLine();
            SelectAndRunAction(subActionMap);
        }
        void ShowItemInfo()
        {
            Console.Clear();
            for (int i = 0; i < InventoryManager.Inventory.Count; i++)
            {
                Item item = InventoryManager.Inventory[i];
                StringBuilder sb = new StringBuilder();
                sb.Append($"{PadRightWithKorean($"- {i + 1}", 5)}");


                sb.Append($"{PadRightWithKorean($"{item.Name}", 18)}");
                for (int j = 0; j < item.Stats.Count; j++)
                {
                    sb.Append($" | {PadRightWithKorean($"{item.Stats[j].GetStatName()} +{item.Stats[j].FinalValue}", 10)} ");

                }
                sb.Append($"| {PadRightWithKorean($"{item.Cost * 0.85}G", 5)}");

                Console.WriteLine(sb.ToString());
                Console.ResetColor();
            }
        }
        public void SetFeedBackMessage(string _msg)
        {
            FeedBackMessage = _msg;
        }
    }
    public class SellAction : ActionBase
    {
        Item item;
        public override string Name => $"{item.Name} 판매하기";
        public SellAction(Item _item, IAction _prevAction)
        {
            PrevAction = _prevAction;
            item = _item;
        }
        public override void OnExcute()
        {
            playerInfo.Gold += (int)(item.Cost * 0.85);
            InventoryManager.RemoveItem(item);

            ((SellItemAction)PrevAction!).SetFeedBackMessage($"{item.Name}을 판매했습니다. (보유골드 : {playerInfo.Gold})");
            PrevAction.Excute();
        }
    }
    public class EnterInventoryAction : ActionBase, IHasSubActions
    {
        public override string Name => "인벤토리";

        public EnterInventoryAction(IAction _prevAction)
        {
            PrevAction = _prevAction;
            subActionMap = new Dictionary<int, IAction>()
                {
                    {1, new EquipItemManagementAction(this) },
                };
        }
        public override void OnExcute()
        {
            Console.Clear();
            Console.WriteLine("보유 중인 아이템을 관리할 수 있습니다.");

            Console.WriteLine("[아이템 목록]");
            for (int i = 0; i < InventoryManager.Inventory.Count; i++)
            {
                Item item = InventoryManager.Inventory[i];
                StringBuilder sb = new StringBuilder();
                sb.Append($"{PadRightWithKorean($"- {i + 1}", 5)}");
                if (item.IsEquipment)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    sb.Append("[E]");
                }

                sb.Append($"{PadRightWithKorean($"{item.Name}", 18)}");
                for (int j = 0; j < item.Stats.Count; j++)
                {
                    sb.Append($" | {PadRightWithKorean($"{item.Stats[j].GetStatName()} +{item.Stats[j].FinalValue}", 10)} ");

                }
                sb.Append(" | ");
                sb.Append($"{PadRightWithKorean($"{item.Descript}", 50)}");


                Console.WriteLine(sb.ToString());
                Console.ResetColor();

            }
            Console.WriteLine();
            SelectAndRunAction(subActionMap);
        }

        IAction IHasSubActions.GetSubAction(int _key)
        {
            return subActionMap[_key];
        }
    }
    public class EquipItemManagementAction : ActionBase, IHasSubActions
    {
        public override string Name => "인벤토리 - 장착 관리";

        public EquipItemManagementAction(IAction _prevAction)
        {
            PrevAction = _prevAction;
        }
        public override void OnExcute()
        {
            Console.Clear();
            Console.WriteLine("보유 중인 아이템을 장착할 수 있습니다.");

            Console.WriteLine("[아이템 목록]");
            Console.WriteLine();

            for (int i = 0; i < InventoryManager.Inventory.Count; i++)
            {
                Item item = InventoryManager.Inventory[i];
                StringBuilder sb = new StringBuilder();
                sb.Append($"{PadRightWithKorean($"- {i + 1}", 5)}");
                if (item.IsEquipment)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    sb.Append("[E]");
                }

                sb.Append($"{PadRightWithKorean($"{item.Name}", 18)}");
                for (int j = 0; j < item.Stats.Count; j++)
                {
                    sb.Append($" | {PadRightWithKorean($"{item.Stats[j].GetStatName()} +{item.Stats[j].FinalValue}", 10)} ");

                }
                sb.Append(" | ");
                sb.Append($"{PadRightWithKorean($"{item.Descript}", 50)}");


                Console.WriteLine(sb.ToString());
                Console.ResetColor();

                if (!subActionMap.ContainsKey(i + 1))
                {
                    subActionMap[i + 1] = new EquipAction(InventoryManager.Inventory[i], this);
                }

            }
            Console.WriteLine();
            SelectAndRunAction(subActionMap);
        }

        IAction IHasSubActions.GetSubAction(int _key)
        {
            return subActionMap[_key];
        }
    }
    public class EquipAction : ActionBase
    {
        Item item;
        public override string Name => $"{item.Name} 아이템 {(item.IsEquipment ? "해제" : "장착")}";

        public EquipAction(Item _item, IAction _prevAction)
        {
            item = _item;
            PrevAction = _prevAction;
        }

        public override void OnExcute()
        {
            Console.Clear();
            string message = string.Empty;
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
            PrevAction?.Excute();
        }
    }

    public class EnterRestAction : ActionBase
    {
        public override string Name => "휴식 하기";

        public EnterRestAction(IAction _prevAction)
        {
            PrevAction = _prevAction;
            subActionMap = new Dictionary<int, IAction>()
                {
                    { 1, new RecoverAction(this) },

                };
        }
        public override void OnExcute()
        {
            Console.WriteLine($"500G를 내면 체력을 회복할 수 있습니다. (보유 골드 : {playerInfo.Gold})");

            SelectAndRunAction(subActionMap);
        }
    }
    public class RecoverAction : ActionBase
    {
        public override string Name => "휴식하기";

        public RecoverAction(IAction _prevAction)
        {
            PrevAction = _prevAction;
        }
        public override void OnExcute()
        {
            string message = string.Empty;
            if (playerInfo.Gold < 500)
            {
                message = "골드가 부족합니다.";
            }
            else
            {
                Stat curHP = playerInfo.Stats[StatType.CurHP];
                curHP.ModifyBaseValue(100, 0, playerInfo.Stats[StatType.MaxHP].FinalValue);
                message = $"체력이 회복되었습니다 현재 체력 : {curHP.FinalValue}";
            }

            PrevAction.Excute();
        }
    }
    public class EnterDungeonAction : ActionBase
    {
        public override string Name => "던전 입장";

        public EnterDungeonAction(IAction _prevAction)
        {
            PrevAction = _prevAction;
            subActionMap = new Dictionary<int, IAction>();
            for (int i = 0; i < DungeonTable.dungeonDic.Count; i++)
            {
                subActionMap.Add(i + 1, new DungeonAction(i + 1, this));
            }
        }
        public override void OnExcute()
        {
            Console.Clear();
            Console.WriteLine("");

            Console.WriteLine("[던전 목록]");
            SelectAndRunAction(subActionMap);
        }

        public void SetFeedBackMessage(string _msg)
        {
            FeedBackMessage = _msg;
        }
    }
    public class DungeonAction : ActionBase
    {
        int dunNum = 0;
        public Dungeon? Dungeon { get; private set; }
        public override string Name => SetName();

        IAction clearDungeonAction;
        IAction failDungeonAction;

        public DungeonAction(int _dunNum, IAction _prevAction)
        {
            dunNum = _dunNum;
            Dungeon = DungeonTable.GetDungeonByID(dunNum);
            PrevAction = _prevAction;
        }
        public override void OnExcute()
        {
            string message = string.Empty;
            Stat dungeonStat = Dungeon.RecommendedStat;
            int playerStat = playerInfo.Stats[dungeonStat.Type].FinalValue;
            StringBuilder sb =new StringBuilder();
            //권장 방어력이 높으면
            if (dungeonStat.FinalValue > playerStat)
            {
                //던전을 실패할 수있음 40프로 확률
                Random rans = new Random();
                float percent = rans.NextSingle();
                if (percent <= 0.4f)
                {
                    message = Dungeon.UnClearDungeon();
                }
                else
                {
                    message = Dungeon.ClearDungeon();
                }
            }
            else
            {
                message = Dungeon.ClearDungeon();
            }


            ((EnterDungeonAction)PrevAction)!.SetFeedBackMessage(message);
            PrevAction?.Excute();

        }

        string SetName()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{PadRightWithKorean($"{Dungeon.DungeonName}", 12)} ");

            sb.Append(PadRightWithKorean($"| {Dungeon.RecommendedStat.GetStatName()}", 10));
            sb.Append(PadRightWithKorean($"+{Dungeon.RecommendedStat.FinalValue} 이상", 10));

            sb.Append(" |");



            return sb.ToString();
        }
    }
}
