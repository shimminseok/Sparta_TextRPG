using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camp_FourthWeek_Basic_C__
{
    internal static class GameManager
    {
        public static PlayerInfo? PlayerInfo { get; private set; }

        public static void Init(JobType _job)
        {
            PlayerInfo = new PlayerInfo(_job);
        }
    }
}
