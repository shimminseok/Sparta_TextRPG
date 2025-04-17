using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camp_FourthWeek_Basic_C__
{
    internal static class LevelManager
    {
        public static int ClearDungeonCount { get; private set; } = 0;
        public static int CurrentLevel { get; private set; } = 1;
        public static void Init()
        {
            for (int i = 0; i < ClearDungeonCount; i++)
            {
                UpdateLevel();
            }
        }
        public static void AddClearCount()
        {
            ClearDungeonCount++;
            UpdateLevel();
        }

        public static void UpdateLevel()
        {
            CurrentLevel = ClearDungeonCount + 1;
            GameManager.Instance.PlayerInfo?.Stats[StatType.Attack].ModifyBaseValue(0.5f);
            GameManager.Instance.PlayerInfo?.Stats[StatType.Defense].ModifyBaseValue(1f);
        }
    }
}
