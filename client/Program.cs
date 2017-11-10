using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameServ
{

    public class App
    {

        private static String ip = "127.0.0.1";
        private static int port = 27960;
        public static ClientNetwork net;

        public static void Main(string[] args)
        {
            Game game = new Game();

            if (args.Length >= 2)
            {
                ip = args[0];
                if (Int32.TryParse(args[1], out port))
                {
                    Console.WriteLine("Bad argument for port. taking default {27860}");
                    port = 27960;
                }
            }
            else
                Console.WriteLine("No arguments given, taking default ip/port { localhost | 27960 }");
            net = new ClientNetwork(game, ip, port);
            Console.WriteLine("Sent hello msg to server");
            game.waitLoop();
            net.clearCon();
        }
    }
}