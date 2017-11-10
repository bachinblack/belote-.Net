using System;

namespace CardGameServ
{

    public class Cards
    {

        private static string[] _Value = { "Ace", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King" };
        private static string[] _Color = { "Heart", "Diamond", "Club", "Spade" };
        private static char[] _Deck = new char[32];

        private static short[] _NAValue = { 11, 0, 0, 0, 10, 2, 3, 4 };
        private static short[] _AValue = { 11, 0, 0, 14, 10, 20, 3, 4 };
        private static char _trump = (char)5;

        public string tostring(char id) { return _Value[id / 4] + " of " + _Color[id / 4]; }

        public static char[] getHand(int i)
        {
            char[] hand = new char[8];
            int k = 7;

            i = i * 8 - 1;
            for (int j = i + 8; j > i; --j)
            {
                hand[k] = _Deck[j];
                --k;
            }
            return hand;
        }

        public static void fillDeck()
        {
            char k = (char)0;

            for (char i = (char)0; i < 32; ++i)
            {
                _Deck[k++] = i;
            }
            shuffleArray();
        }

        static void shuffleArray()
        {
            char a;
            Random rnd = new Random();

            for (char i = (char)(_Deck.Length - 1); i > 0; --i)
            {
                char index = (char)rnd.Next(i + 1);
                a = _Deck[index];
                _Deck[index] = _Deck[i];
                _Deck[i] = a;
            }
        }

        public static int getScore(char id) { return (id % 4 == _trump ? _AValue[id / 4] : _NAValue[id / 4]); }

        public static string colTostring(char c) { return _Color[c]; }

        public static void setTrump(char nt) { _trump = nt; }
    }
}