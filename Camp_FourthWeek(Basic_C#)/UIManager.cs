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


    internal static class UIManager
    {
        public static StringBuilder ItemPrinter(Item _item, int _index = -1, bool _isShowDescript = true)
        {
            StringBuilder sb = new StringBuilder();

            if (_index >= 0)
            {
                sb.Append($"{PadRightWithKorean($"- {_index + 1}", 5)}"); //아이템 이름


                sb.Append($"{PadRightWithKorean(_item.Name, 18)} | ");
                StringBuilder statBuilder = new StringBuilder();
                foreach (Stat stat in _item.Stats)
                {
                    statBuilder.Append($"{PadRightWithKorean($"{stat.GetStatName()}",10)}");
                    statBuilder.Append($"{PadRightWithKorean($"+{stat.FinalValue}",5)}");
                }
                sb.Append($"{PadRightWithKorean(statBuilder.ToString(), 35)}");
                if (_isShowDescript)
                    sb.Append($" | {PadRightWithKorean(_item.Descript,50)}");
            }
            return sb;
        }
    }
}
