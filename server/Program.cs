using System;

namespace CardGameServ
{

    public class App
    {

        private static int port = 27960;

        public static void Main(string[] args)
        {

            WaitingRoom room = new WaitingRoom();
            ServerNetwork net;

            if (args.Length >= 1)
            {
                if (Int32.TryParse(args[0], out port))
                    Console.WriteLine("Using port { " + args[0] + " }.");
            }
            else
            {
                Console.WriteLine("No arguments given, taking default port { 27960 }.");
            }

            net = new ServerNetwork(room, port);

            Console.WriteLine("Launching waiting loop");
            room.waitForGame();
        }
    }
}