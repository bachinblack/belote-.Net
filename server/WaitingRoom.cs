using System;
using System.Threading;
using System.Collections.Generic;
using NetworkCommsDotNet.Connections;

namespace CardGameServ
{

    public class Player
    {
        public string name;
        public Connection con;
        public char[] played = new char[32];

        public Player(Connection _con, string _name)
        {
            con = _con;
            name = _name;
        }
    }

    public class ThreadList<T>
    {
        private List<T> _list = new List<T>();
        private object _sync = new object();

        public void Add(T value)
        {
            lock (_sync)
            {
                _list.Add(value);
            }
        }

        public T Get(int index)
        {
            lock (_sync)
            {
                if (_list.Count > index)
                    return (_list[index]);
                return (_list[0]);
            }
        }

        public void Remove(int index)
        {
            lock (_sync)
            {
                if (index < _list.Count)
                    _list.RemoveAt(index);
            }
        }

        public int Size()
        {
            return (_list.Count);
        }
    }

    class WaitingRoom
    {
        private ThreadList<Player> players = new ThreadList<Player>();
        private List<GameInstance> gth = new List<GameInstance>();

        public void AddPlayer(Connection con, string name)
        {
            Msg msg = new Msg();
            Console.WriteLine("Adding " + name);

            msg.text = AddDate(name + " joined the room. There are now " + (players.Size() + 1) + "/4 players in the room.");
            for (int i = 0; i < players.Size(); ++i)
            {
                players.Get(i).con.SendObject<Msg>("Msg", msg);
            }
            players.Add(new Player(con, name));
            if (players.Size() >= 4)
                AddGame();
        }
         
        public bool isPlayerRegistered(string name)
        {
            for (int i=0; i<players.Size(); ++i)
            {
                if (players.Get(i).name.Equals(name))
                    return true;
            }
            return false;
        }

        public void sendChatMessage(Msg msg)
        {
            msg.text = AddDate(msg.text);
            for (int i = 0; i < players.Size(); ++i)
                //foreach (Player p in players)
            {
                players.Get(i).con.SendObject<Msg>("Msg", msg);
            }
        }

        public static string AddDate(string msg)
        {
            msg = DateTime.Now.ToString("[HH:mm:ss] ") + msg;
            return msg;
        }

        public void removePlayer(Connection con)
        {
            Msg msg = new Msg();
            bool found = false;

            for (int i = 0; i < players.Size(); ++i)
            {
                if (players.Get(i).con == con)
                {
                    msg.text = AddDate(players.Get(i).name + " left the room. There are now " + (players.Size() - 1) + "/4 players in the room.");
                    players.Remove(i);
                    found = true;
                }
            }
            if (!found)
            {
                foreach (GameInstance th in gth)
                {
                    if ((th.seekPlayer(con)))
                        break;

                }
            }
            if (msg.text == null) return;
            for (int i = 0; i < players.Size(); ++i)
            //foreach (Player p in players)
            {
                players.Get(i).con.SendObject<Msg>("Msg", msg);
            }
        }

        private void AddGame()
        {
            GameInstance gt = new GameInstance(players.Get(0), players.Get(1), players.Get(2), players.Get(3));

            gt.setId((char)gth.Count);
            gth.Add(gt);
            players.Remove(0);
            players.Remove(0);
            players.Remove(0);
            players.Remove(0);
            gt.run();
        }

        public void waitForGame()
        {
            int i;

            while (true)
            {
                i = 0;
                Console.WriteLine("There are " + players.Size() + " players in the room. " + gth.Count + " games are currently being played.");
                while (i < gth.Count)
                {
                    if (gth[i].gameOver)
                    {
                        appendPlayers(gth[i].getPlayers());
                        gth.RemoveAt(i);
                        if (players.Size() >= 4)
                            AddGame();
                    }
                    ++i;
                }
                Thread.Sleep(4000);
            }
        }

        private void appendPlayers(Player[] np)
        {
            foreach (Player p in np)
            {
                if (!p.name.Equals(""))
                    players.Add(p);
            }
        }

        public void dispatchCommand(GameCommand com)
        {
            foreach (GameInstance gt in gth)
            {
                if (gt.getId() == com.meta)
                {
                    gt.processResponse(com.data);
                    break;
                }
            }
        }
    }
}