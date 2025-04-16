namespace Camp_FourthWeek_Basic_C__
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using static Camp_FourthWeek_Basic_C__.Program;
    
 

    internal class Program
    {
        static void Main(string[] args)
        {
            TextRPG.StartGame();
        }
        
        #region[Interface]

        #endregion[Interface]
        #region[Action Class]
        

        #endregion[Action Class]


        public static class TextRPG
        {
            public static void StartGame()
            {
                GameManager.Init(JobType.Warrior);
                var mainAction = new MainMenuAction();
                mainAction.InitializeMainActions(mainAction);
                mainAction.Excute();
            }
        }
    }
}

