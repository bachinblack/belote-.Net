using System;

namespace CardGameServ
{

    public class Cards
    {

        private static string[] _Value = { "Ace", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King" };
        private static string[] _Color = { "Heart", "Diamond", "Club", "Spade" };
        public static char[] _Deck = new char[32];

        public static string tostring(char id) { return _Value[id / 4] + " of " + _Color[id % 4]; }

        public static char toColor(string col)
        {
            char it = (char)0;
            foreach (string s in _Color)
            {
                if (s.Equals(col, StringComparison.CurrentCultureIgnoreCase))
                    return it;
                ++it;
            }
            Console.WriteLine("No color correspond to your choice, please retry.");
            return (char)6;
        }

        public static string colTostring(char c) { return _Color[c]; }
    }
}