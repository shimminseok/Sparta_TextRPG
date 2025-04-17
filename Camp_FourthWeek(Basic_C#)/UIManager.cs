using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Camp_FourthWeek_Basic_C__.StringUtil;

namespace Camp_FourthWeek_Basic_C__
{
    public static class StringUtil
    {
        public static string PadRightWithKorean(string _str, int _totalWidth)
        {
            int width = _str.Sum(c => IsKorean(c) ? 2 : 1);

            int padding = _totalWidth - width;
            StringBuilder sb = new StringBuilder();
            sb.Append(_str);
            if (padding > 0)
            {
                sb.Append(new string(' ', padding));
                return sb.ToString();
            }

            return sb.ToString();
        }

        private static bool IsKorean(char _c)
        {
            return (_c >= 0xAC00 && _c <= 0xD7A3);
        }
    }


    internal static class UiManager
    {
        public static StringBuilder ItemPrinter(Item _item, int _index = -1, bool _isShowDescript = true)
        {
            var sb = new StringBuilder();

            if (_index < 0) return sb;
            sb.Append($"{PadRightWithKorean($"- {_index + 1}", 5)}"); //아이템 이름


            sb.Append($"{PadRightWithKorean(_item.Name, 18)} | ");
            var statBuilder = new StringBuilder();
            foreach (var stat in _item.Stats)
            {
                statBuilder.Append($"{PadRightWithKorean($"{stat.GetStatName()}", 10)}");
                statBuilder.Append($"{PadRightWithKorean($"+{stat.FinalValue}", 5)}");
            }

            sb.Append($"{PadRightWithKorean(statBuilder.ToString(), 35)}");
            if (_isShowDescript)
                sb.Append($" | {PadRightWithKorean(_item.Description, 50)}");
            return sb;
        }

        public static void PrintStat(Stat _stat)
        {
            Console.WriteLine($"{StringUtil.PadRightWithKorean(_stat.GetStatName(), 15)} : {_stat.FinalValue}");
        }

        public static void PrintHeager(string _title)
        {
            Console.WriteLine("\n===============================");
            Console.WriteLine(_title);
            Console.WriteLine("===============================");
        }

        public static void PrintError(string _message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[오류] {_message}");
            Console.ResetColor();
        }

        public static void PrintSuccess(string _message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[성공] {_message}");
            Console.ResetColor();
        }
    }
}