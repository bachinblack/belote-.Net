using System;
using System.Threading;
using NetworkCommsDotNet.Connections;

namespace CardGameServ
{

    public class GameInstance
    {
        private Player[] players = new Player[4];
        private char[] trump = { (char)0x5, (char)0 };
        private char playerBet = (char)0;
        private char cbIt = (char)0;
        private char[] board = { (char)32, (char)32, (char)32, (char)32 };
        private short Atotal = 0, Btotal = 0;
        public  bool gameOver = false;
        private char id;

        public GameInstance(Player player1, Player player2, Player player3, Player player4)
        {
            players[0] = player1;
            players[1] = player2;
            players[2] = player3;
            players[3] = player4;
            Cards.fillDeck();
        }

        public char getId() { return id; }

        public void setId(char lid) { id = lid; }

        public void run()
        {
            Console.WriteLine("starting Game. id = " + (short)id);
            GameCommand cmd = new GameCommand();
            InitMsg msg = new InitMsg();
            int i;

            Console.WriteLine("player 1: " + players[0].name);
            Console.WriteLine("player 2: " + players[1].name);
            Console.WriteLine("player 3: " + players[2].name);
            Console.WriteLine("player 4: " + players[3].name);
            msg.threadId = id;
            msg.players = new string[4];
            for (i = 0; i < 4; ++i)
                msg.players[i] =
                    players[i].name;

            for (i = 0; i < 4; ++i)
            {
                msg.cards = Cards.getHand(i);
                players[i].con.SendObject<InitMsg>("InitMsg", msg);
            }

            Thread.Sleep(100);
            cmd.meta = (char)0;
            cmd.data = trump;
            players[0].con.SendObject<GameCommand>("GameCommand", cmd);
            // !! Add a while that check if a client can answer in less than 15 s. If it can't, leave the game
        }

        // trump is the left byte, bet is the right one.
        private char[] choseTrump(char[] data)
        {
            if (data[1] > trump[1])
            {
                trump[0] = data[0];
                trump[1] = data[1];
                playerBet = (char)(cbIt % 4);
            }
            return trump;
        }

        private char[] play(char data)
        {
            int i, winner = 0, best = 0, mod = cbIt % 4;
            int score;

            players[mod].played[cbIt / 4 - 2] = data;
            board[mod] = data;
            if (mod == 3)
            {
                // checking for the best score
                for (i = 0; i < 4; ++i)
                {
                    if ((score = Cards.getScore(board[i])) > best)
                    {
                        best = score;
                        winner = i;
                    }
                }
                setScore(winner);
                // !! send the 4 cards played this round to everyone
                for (i = 0; i < 4; ++i)
                {
                    board[i] = (char)32;
                }
            }
            return board;
        }

        private void setScore(int winner)
        {
            int i, keep = 0;

            for (i = 0; i < 4; ++i)
                keep += Cards.getScore(board[i]);
            if (cbIt == 39)
                keep += 10;
            if (winner == 0 || winner == 2)
                Atotal += (short)keep;
            else
                Btotal += (short)keep;
            sendToAll(players[winner].name + " won this round. His team earned " + keep + " points" + (cbIt == 39 ? " + 10 de der." : "."));
        }

        private void getBeloteRebelote()
        {
            char gotOne = (char)0, i = (char)0;

            foreach (Player p in players)
            {
                foreach (char c in p.played)
                {
                    if (c == 24 + trump[0] || c == 28 + trump[0])
                    {
                        if (gotOne == 0)
                            ++gotOne;
                        else if (i == 0 || i == 2)
                        {
                            Atotal += 20;
                            sendToAll("The team A earned 20 points by belote-rebelote thanks to " + p.name);
                        }
                        else
                        {
                            Btotal += 20;
                            sendToAll("The team B earned 20 points by belote-rebelote thanks to " + p.name);
                        }
                    }
                }
                gotOne = (char)0;
                ++i;
            }
        }

        private void FinalCount()
        {

            if (playerBet == 0 || playerBet == 2 && Atotal >= trump[1])
            {
                sendToAll("Team A marked " + Atotal + " points. The contract of " + (short)trump[1] + " points was fulfilled.");
                Atotal += (short)trump[1];
            }
            else if (Btotal >= trump[1])
            {
                sendToAll("Team B marked " + Btotal + " points. The contract of " + (short)trump[1] + " points was fulfilled.");
                Btotal += (short)trump[1];
            }
            getBeloteRebelote();
            if (Atotal > Btotal)
            {
                if (Btotal == 0)
                {
                    Atotal += 90;
                    sendToAll("The team A won by capot. 10 de der is now 100 de der.");
                }
                sendToAll("The team A won this game by " + Atotal + " to " + Btotal + ".");
            }
            else
            {
                if (Atotal == 0)
                {
                    Btotal += 90;
                    sendToAll("The team B won by capot. 10 de der is now 100 de der.");
                }
                sendToAll("The team of B won this game by " + Btotal + " to " + Atotal + ".");

            }
            closeThread();
        }

        public void processResponse(char[] data)
        {
            GameCommand cmd = new GameCommand();
            int toDiv = cbIt / 4;

            switch (toDiv)
            {
                case 0:
                    cmd.data = choseTrump(data);
                    break;
                case 1:
                    cmd.data = choseTrump(data);
                    break;
                default:
                    cmd.data = play(data[0]);
                    break;
            }
            if (++cbIt == 40)
                FinalCount();
            else
            {
                if (cbIt == 8)
                {
                    if (trump[0] == 5)
                    {
                        sendToAll("Nobody did bet this time :( Sending you punks back to the waiting room.");
                        closeThread();
                        return;
                    }
                    else
                        sendToAll("The highest bet is " + (int)trump[1] + ". The trump color is " + Cards.colTostring(trump[0]) + ".");
                    Cards.setTrump(trump[0]);
                    cmd.data = board;
                }
                cmd.meta = (char)(cbIt < 8 ? 0 : 1);
                players[cbIt % 4].con.SendObject<GameCommand>("GameCommand", cmd);
            }
        }

        private void sendToAll(string s)
        {
            Msg msg = new Msg();

            msg.text = WaitingRoom.AddDate(s);
            foreach (Player p in players)
            {
                p.con.SendObject<Msg>("Msg", msg);
            }
        }

        private void closeThread()
        {
            sendToAll("The game is over, you are gonna be sent back to the waiting room to play an other game...");
            gameOver = true;
        }

        public bool seekPlayer(Connection con)
        {
            foreach (Player p in players)
            {
                if (p.con == con)
                {
                    sendToAll("The player " + p.name + " left the game.");
                    closeThread();
                    p.name = "";
                    return true;
                }
            }
            return false;
        }

        public Player[] getPlayers() { return players; }
    }
}