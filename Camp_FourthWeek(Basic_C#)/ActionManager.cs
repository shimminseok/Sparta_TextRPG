using System;
using System.Collections.Generic;
using System.Text;
using static Camp_FourthWeek_Basic_C__.StringUtil;
using static System.Net.Mime.MediaTypeNames;

namespace Camp_FourthWeek_Basic_C__
{

    /// <summary>
    /// 모든 Action의 부모 클래스이며 모든 Action은 해당 메서드를 참조받아야함.
    /// </summary>
    public abstract class ActionBase : IAction
    {
        public IAction? PrevAction { get; protected set; }
        public abstract string Name { get; }

        protected PlayerInfo playerInfo = GameManager.PlayerInfo;
        protected Dictionary<int, IAction> subActionMap = new Dictionary<int, IAction>();
        string FeedBackMessage = string.Empty;
        public abstract void OnExcute();

        public void SelectAndRunAction(Dictionary<int, IAction> _actionMap)
        {
            Console.WriteLine();
            foreach (var action in _actionMap)
            {
                Console.WriteLine($"{action.Key}. {action.Value.Name}");
            }
            Console.WriteLine();
            Console.WriteLine($"0. {(PrevAction == null ? "종료하기" : $"{PrevAction.Name}로 되돌아가기")}");
            Console.WriteLine();
            Console.WriteLine(FeedBackMessage);
            Console.WriteLine("원하시는 행동을 입력해주세요.");
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out var id))
                {
                    if (id == 0)
                    {
                        if (PrevAction == null && this is MainMenuAction)
                        {
                            GameManager.SaveGame();
                        }
                        else
                        {
                            PrevAction?.Excute();
                        }
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
            FeedBackMessage = string.Empty;

        }
        public void Excute()
        {
            Console.WriteLine($"['{Name}]");
            OnExcute();
        }

        public void SetFeedBackMessage(string _message)
        {
            FeedBackMessage = _message;
        }
    }
    public class CreateCharacterAction : ActionBase
    {
        public override string Name => "캐릭터 생성";

        public CreateCharacterAction()
        {
            subActionMap = new Dictionary<int, IAction>()
            {
                {1, new CreateNickNameAction() }
            };
        }
        public override void OnExcute()
        {
            Console.WriteLine("스파르타마을에 오신 모험가님을 환영합니다.");
            SelectAndRunAction(subActionMap);
        }
    }
    public class CreateNickNameAction : ActionBase
    {
        public override string Name => "닉네임 설정";

        string? nickName = string.Empty;
        public override void OnExcute()
        {
            subActionMap.Clear();
            do
            {
                Console.WriteLine("모험가님의 이름을 설정해주세요.");
                nickName = Console.ReadLine();
            } while (string.IsNullOrEmpty(nickName));
            Console.WriteLine($"{nickName}님 안녕하세요.");
            subActionMap = new Dictionary<int, IAction>
                {
                    {1,  new SelectedJobAction(nickName)}
                };
            SelectAndRunAction(subActionMap);

        }
    }
    public class SelectedJobAction : ActionBase
    {
        public override string Name => "직업 선택";

        public SelectedJobAction(string _name)
        {
            foreach (var job in JobTable.JobDataDic.Values)
            {
                subActionMap.Add((int)job.Type, new SelectJobAction(job, _name));
            }
        }
        public override void OnExcute()
        {
            Console.WriteLine($"플레이 하실 직업을 선택해주세요.");
            Console.WriteLine();
            int index = 1;
            foreach (var job in JobTable.JobDataDic.Values)
            {
                Console.Write($"\t{job.Name}");
            }
            Console.WriteLine();
            SelectAndRunAction(subActionMap);
        }
    }
    public class SelectJobAction : ActionBase
    {
        public override string Name => Job.Name;
        public Job Job { get; private set; }

        public string CharacterName { get; private set; }
        public SelectJobAction(Job _job, string _name)
        {
            Job = _job;
            CharacterName = _name;
        }
        public override void OnExcute()
        {
            Console.WriteLine($"직업 : {Job.Name}을 선택하셨습니다.");

            Console.WriteLine("잠시 후 게임이 시작됩니다.");
            GameManager.Init(Job.Type, CharacterName);
            MainMenuAction main = new MainMenuAction();
            Thread.Sleep(1000);
            main.InitializeMainActions(main);
            main.Excute();
        }
    }
    public class MainMenuAction : ActionBase
    {
        public override string Name => "마을";

        public void InitializeMainActions(MainMenuAction mainAction)
        {
            subActionMap.Clear();
            subActionMap[(int)Menu.ShowInfo] = new EnterCharacterInfoAction(mainAction);
            subActionMap[(int)Menu.Inventory] = new EnterInventoryAction(mainAction);
            subActionMap[(int)Menu.Shop] = new EnterShopAction(mainAction);
            subActionMap[(int)Menu.Dungeon] = new EnterDungeonAction(mainAction);
            subActionMap[(int)Menu.Rest] = new EnterRestAction(mainAction);
            subActionMap[(int)Menu.Reset] = new EnterResetAction(mainAction);
        }

        public override void OnExcute()
        {
            Console.WriteLine("스파르타 마을에 오신 여러분 환영합니다.");
            Console.WriteLine("이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.");

            SelectAndRunAction(subActionMap);
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
            Console.WriteLine("캐릭터의 정보가 표시됩니다.");
            Console.WriteLine();

            Console.WriteLine($"Lv. {LevelManager.CurrentLevel} 이름 : {playerInfo.Name}");
            Console.WriteLine(PadRightWithKorean($"{PadRightWithKorean("직업", 9)} : {playerInfo.Job.Name}", 10));
            foreach (var stat in playerInfo.Stats.Values)
            {
                Console.WriteLine($"{PadRightWithKorean(stat.GetStatName(), 9)} : {PadRightWithKorean(stat.FinalValue.ToString("N0"), 9)} +({stat.EquipmentValue})");
            }
            Console.WriteLine($"{PadRightWithKorean("Gold", 9)} : {playerInfo.Gold}");
            Console.WriteLine();

            SelectAndRunAction(subActionMap);
        }
    }
    public class EnterShopAction : ActionBase
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
            Console.WriteLine("[보유 골드]");
            Console.WriteLine($"{playerInfo.Gold}G");
            Console.WriteLine();
            Console.WriteLine("[아이템 목록]");
            for (int i = 0; i < SaleItems.Count; i++)
            {
                Item item = SaleItems[i];
                StringBuilder sb = UIManager.ItemPrinter(item, i);
                sb.Append(" | ");
                if (InventoryManager.Inventory.Exists(x => x.Name == item.Name))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    sb.Append($"{PadRightWithKorean($"구매완료", 10)}");
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
        public override string Name => "상점 - 아이템 구매";
        List<Item> SaleItems = new List<Item>();

        public BuyItemAction(IAction _prevAction)
        {
            PrevAction = _prevAction;
        }
        public override void OnExcute()
        {
            SaleItems = ItemTable.itemDic.Values.Where(item => !InventoryManager.Inventory.Any(x => x.Key == item.Key)).ToList();
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
            for (int i = 0; i < SaleItems.Count; i++)
            {
                Item item = SaleItems[i];
                StringBuilder sb = UIManager.ItemPrinter(item, i);
                if (InventoryManager.Inventory.Exists(x => x.Name == item.Name))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    sb.Append($"{PadRightWithKorean($"구매완료", 10)}");
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

            PrevAction?.SetFeedBackMessage(message);
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
            for (int i = 0; i < InventoryManager.Inventory.Count; i++)
            {
                Item item = InventoryManager.Inventory[i];
                StringBuilder sb = UIManager.ItemPrinter(item, i, false);
                sb.Append($"| {PadRightWithKorean($"{item.Cost * 0.85}G", 5)}");

                Console.WriteLine(sb.ToString());
                Console.ResetColor();
            }
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

            PrevAction!.SetFeedBackMessage($"{item.Name}을 판매했습니다. (보유골드 : {playerInfo.Gold})");
            PrevAction.Excute();
        }
    }
    public class EnterInventoryAction : ActionBase
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
            Console.WriteLine("보유 중인 아이템을 관리할 수 있습니다.");

            Console.WriteLine("[아이템 목록]");
            for (int i = 0; i < InventoryManager.Inventory.Count; i++)
            {
                Item item = InventoryManager.Inventory[i];
                StringBuilder sb = UIManager.ItemPrinter(item, i);

                if (item.IsEquipment)
                {
                    int idx = sb.ToString().IndexOf(item.Name);
                    Console.ForegroundColor = ConsoleColor.Green;
                    sb.Insert(idx, "[E]");
                }
                Console.WriteLine(sb.ToString());
                Console.ResetColor();

            }
            SelectAndRunAction(subActionMap);
        }
    }
    /// <summary>
    /// 장착을 관리하는 Action
    /// </summary>
    public class EquipItemManagementAction : ActionBase
    {
        public override string Name => "인벤토리 - 장착 관리";

        public EquipItemManagementAction(IAction _prevAction)
        {
            PrevAction = _prevAction;
        }
        public override void OnExcute()
        {
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
    }
    /// <summary>
    /// 아이템을 장착하는 Action
    /// </summary>
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
                float before = curHP.FinalValue;
                curHP.ModifyBaseValue(playerInfo.Stats[StatType.MaxHP].FinalValue, 0, playerInfo.Stats[StatType.MaxHP].FinalValue);
                message = $"체력이 회복되었습니다 HP {before} -> {curHP.FinalValue}";
            }

            PrevAction!.SetFeedBackMessage(message);
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
            Console.WriteLine("");

            Console.WriteLine("[던전 목록]");
            Console.WriteLine($"현재 공격력 : {playerInfo.Stats[StatType.Attack].FinalValue}");
            Console.WriteLine($"현재 방어력 : {playerInfo.Stats[StatType.Defense].FinalValue}");

            SelectAndRunAction(subActionMap);
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
            //던전 도전 조건 판단
            Stat dungeonStat = Dungeon.RecommendedStat;
            float playerStat = playerInfo.Stats[dungeonStat.Type].FinalValue;

            if(35 - playerStat - dungeonStat.FinalValue > playerInfo.Stats[StatType.CurHP].FinalValue)
            {
                PrevAction!.SetFeedBackMessage("체력이 부족합니다.");
                PrevAction?.Excute();
                return;
            }

            Random rans = new Random();
            float damage = rans.Next(20, 36);
            damage -= playerStat - dungeonStat.FinalValue;



            string message = string.Empty;
            StringBuilder sb = new StringBuilder();

            //권장 방어력이 높으면
            if (dungeonStat.FinalValue > playerStat)
            {
                //던전을 실패할 수있음 40프로 확률
                float percent = rans.NextSingle();
                if (percent <= 0.4f)
                {
                    message = Dungeon.UnClearDungeon(damage);
                }
                else
                {
                    message = Dungeon.ClearDungeon(damage);
                }
            }
            else
            {
                message = Dungeon.ClearDungeon(damage);
            }


            PrevAction!.SetFeedBackMessage(message);
            PrevAction!.Excute();

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
    public class EnterResetAction : ActionBase
    {
        public override string Name => "리셋!!!";

        public EnterResetAction(IAction _action)
        {
            PrevAction = _action;
        }
        public override void OnExcute()
        {
            Console.WriteLine("저장된 데이터를 삭제하시겠습니까?");
            Console.WriteLine("1. 예 \t 2.아니오");

            if(int.TryParse(Console.ReadLine(), out var input))
            {
                if(input == 1)
                {
                    GameManager.DeleteGameData();
                }
                else if(input == 2)
                {
                    PrevAction?.Excute();
                }
            }
        }
    }
    public class TestAction : ActionBase
    {
        public override string Name => "이건 테스트에오";

        public TestAction(IAction _prevAction)
        {
            PrevAction = _prevAction;
        }
        public override void OnExcute()
        {
            Console.WriteLine("테스트 액션 함수 입니다.");
            Console.WriteLine("OnExcute에서 해당 액션에 대한 행동을 구현 하고");
            Console.WriteLine("해당 Action을 실행 시킬 상위 Action에 등록만 해주면 바로 사용이 가능합니다.");
            Console.WriteLine("상위 Action에서 해당 Action 등록할 때 상위 Action의 IAction Interface를 참조 받아서 사용합니다.");
            Console.WriteLine("이렇게 하게 되면 상위 Action과의 클래스 결합도가 낮아져서 유지보수가 쉬워집니다.");
            Console.WriteLine("사실 상위 액션을 상속 받아 쓰면 되는거 아니냐?? 라고 생각 하실수도 있지만");
            Console.WriteLine("상위 액션을 상속받아 하위 액션을 구현하게되면 하위 액션은 쓸모없는것까지 상속 받게 되어 비효율적입니다.");
            Console.WriteLine("그리고 상위 액션이 수정됨에 따라 하위 액션이 바뀌게되는 위험성이 존재합니다.");


            //PrevAction?.Excute();
        }
    }
}
