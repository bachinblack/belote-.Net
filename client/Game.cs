using System;
using System.IO;
using NetworkCommsDotNet.Connections;

namespace CardGameServ
{

    public class Game
    {
        private char[]      cards;
        private string[]    players = new string[1];
        private char[]      board;
        private short       nCards;
        private short       nBoard;
        private char[]      trump;
        private Connection  con;
        private char        threadId = (char)255;
        private bool        gStart = false;
        private char        cmd = (char)255;

        public Game()
        {
            nCards = 0;
            nBoard = 0;

            Console.WriteLine("Chose a nickname:");
            try
            {
                while ((players[0] = Console.ReadLine()).Equals("")) ;
            }
            catch (IOException)
            {
                Console.WriteLine("failed to read nickname. Leaving...");
                return;
            }
            Console.WriteLine("Okay, your nickname is [" + players[0] + "].");
        }

        private void displayCards()
        {
            int i = 0;
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("You have " + nCards + " cards left" + (nCards == 0 ? "." : ":"));
            foreach (char c in cards)
            {
                if (c != 32)
                {
                    Console.WriteLine(i + ": " + Cards.tostring(c));
                }
                ++i;
            }
            Console.WriteLine("----------------------------------------------");
        }

        private void displayBoard()
        {

            nBoard = 0;
            foreach (char c in board)
            {
                if (c != 32)
                    ++nBoard;
            }
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("There are " + nBoard + " cards on the board" + (nBoard == 0 ? "." : ":"));
            foreach (char c in board)
            {
                if (c != 32)
                    Console.WriteLine("- " + Cards.tostring(c));
            }
            Console.WriteLine("----------------------------------------------");
        }

        private void displayPlayers()
        {
            Console.WriteLine("Here are the players in your room:");

            foreach (string s in players)
            {
                Console.WriteLine("- " + s);
            }
            Console.WriteLine("----------------------------------------------");
        }

        public string getName() { return (players[0]); }

        public void waitLoop()
        {
            Msg msg = new Msg();
            string s;
            char[] data = new char[2];

            try
            {
                while ((s = Console.ReadLine()).Equals("!quit") == false)
                {
                    if (!gStart)
                    {
                        if (s.Length != 0)
                        {
                            msg.text = players[0] + ": " + s;
                            con.SendObject("Msg", msg);
                        }
                    }
                    else if (cmd == 0)
                    {
                        if (s.Length == 0)
                        {
                            data[0] = (char)0;
                            data[1] = (char)0;
                        }
                        else
                        {
                            data = getTrumpValues(s);
                            if (data[0] == 6) { continue; }
                        }
                        sendCommand(data);
                        Console.WriteLine("Your answer has been sent...");
                        cmd = (char)255;
                    }
                    else if (cmd == 1)
                    {
                        if (s.Length == 0)
                        {
                            Console.WriteLine("You can't skip your turn, you have to chose a card.");
                            continue;
                        }
                        if ((data[0] = getCardToPlay(s)) == 32)
                            continue;
                        sendCommand(data);
                        Console.WriteLine("Your answer have been sent...");
                        cmd = (char)255;
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("failed to do smthg");
            }
        }

        private void sendCommand(char[] data)
        {
            GameCommand cmd = new GameCommand();

            cmd.data = data;
            cmd.meta = threadId;
            con.SendObject<GameCommand>("GameCommand", cmd);

        }

        public void receiveCommand(GameCommand com)
        {
            cmd = com.meta;
            switch (com.meta)
            {
                case (char)0:
                    {
                        trump = com.data;
                        if (com.data[0] == 0x5)
                            Console.WriteLine("You are the first to bet (empty for skipping):");
                        else
                            Console.WriteLine("The current bet is: " + Cards.colTostring(com.data[0]) + " and " + (int)com.data[1] + " points. Type your bet (empty for skipping).");
                        Console.WriteLine("Chose a color in this list [Heart - Diamond - Club - Spade] and a score between " + (trump[1] == 0 ? 80 : (int)trump[1]) + " and 250");
                        break;
                    }
                case (char)1:
                    {
                        board = com.data;
                        play();
                        break;
                    }
            }
        }

        private void play()
        {
            displayCards();
            displayBoard();
            Console.WriteLine("Chose one of your cards by its number [0-7].");
        }

        public void InitData(InitMsg msg)
        {
            gStart = true;
            nCards = 8;
            cards = msg.cards;
            players = msg.players;
            threadId = msg.threadId;
            displayPlayers();
            displayCards();
        }

        private char[] getTrumpValues(string s)
        {
            char[] data = { (char)6, (char)0 };
            int subValue = 0;
            int it = 0;

            foreach (char c in s)
            {
                if (c == ' ')
                    subValue = it;
                else if (subValue != 0 && (c < '0' || c > '9'))
                {
                    Console.WriteLine("The second parameter has to be an integer between " + (trump[1] == 0 ? 80 : (int)trump[1]) + " and 250");
                    data[0] = (char)6;
                    return data;
                }
                ++it;
            }
            if (subValue == 0)
                Console.WriteLine("Your answer has to be formatted like this: [trump] [score]");
            else
            {
                Console.WriteLine("trump color is:" + s.Substring(0, subValue));
                data[0] = Cards.toColor(s.Substring(0, subValue));
                subValue = Int32.Parse(s.Substring(subValue + 1));
                if (subValue < (trump[1] == 0 ? 80 : trump[1]) || subValue > 250)
                {
                    data[0] = (char)6;
                    Console.WriteLine("The second parameter has to be an integer between " + (trump[1] == 0 ? 80 : (int)trump[1]) + " and 250");
                }
                else
                    data[1] = (char)subValue;
            }
            return data;
        }

        private char getCardToPlay(string s)
        {
            char keep;
            char ret;

            if (s.Length > 1 || s[0] < '0' || s[0] > '7')
            {
                Console.WriteLine("you have to select a card by its number [0-7].");
                return (char)32;
            }

            keep = (char)Int32.Parse(s);
            if (cards[keep] == 32)
            {
                Console.WriteLine("This card isn't available anymore. You have to chose another one.");
                return (char)32;
            }

            --nCards;
            ret = cards[keep];
            cards[keep] = (char)32;
            return ret;
        }

        public void setCon(Connection ncon) { con = ncon; }
    }
}