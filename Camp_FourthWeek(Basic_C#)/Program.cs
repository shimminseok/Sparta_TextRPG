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
            TextRPG textRPG = new TextRPG();
            textRPG.StartGame();
        }
    }
    public class TextRPG
    {
        public void StartGame()
        {
            GameManager.Instance.LoadGame();
        }
    }
}

