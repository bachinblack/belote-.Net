using System;
using NetworkCommsDotNet.DPSBase;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections.TCP;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Tools;

namespace CardGameServ
{

    public class ClientNetwork
    {
        private Game game;
        private TCPConnection con;
        public ClientNetwork(Game ngame, string ip, int port)
        {
            Hello msg = new Hello();

            SendReceiveOptions customSendReceiveOptions = new SendReceiveOptions<ProtobufSerializer>();
            con = TCPConnection.GetConnection(new ConnectionInfo(ip, port), customSendReceiveOptions);

            NetworkComms.AppendGlobalIncomingPacketHandler<Back>("Back", processBack);
            NetworkComms.AppendGlobalIncomingPacketHandler<Msg>("Msg", processMsg);
            NetworkComms.AppendGlobalIncomingPacketHandler<InitMsg>("InitMsg", processInitMsg);
            NetworkComms.AppendGlobalIncomingPacketHandler<GameCommand>("GameCommand", processGC);

            game = ngame;
            msg.text = game.getName();
            try
            {
                con.SendObject("Hello", msg);
            } catch (ArgumentException) {
                Console.WriteLine("Couldn't send request message. It appears to be an internal error. too bad :/");
            }  catch (ConnectionSetupException)  {
                Console.WriteLine("Couldn't send request message. Maybe the ip address is wrong or the server isn't running?");
            } catch(CommunicationException) {
                Console.WriteLine("Couldn't send request message. Maybe the ip address is wrong or the server isn't running?");
            }
            game.setCon(con);
        }

        public void clearCon()
        {
            if (con.ConnectionAlive())
                con.CloseConnection(false);
        }

        private void processBack(PacketHeader packetHeader, Connection connection, Back msg)
        {
            Console.WriteLine(msg.text);
            if (msg.text.Substring(0, 9).Equals("[!server]")) { Environment.Exit(1); }
        }

        private void processMsg(PacketHeader packetHeader, Connection connection, Msg msg)
        {
            Console.WriteLine("Got chat message: " + msg.text);
        }

        private void processGC(PacketHeader packetHeader, Connection connection, GameCommand msg)
        {
            Console.WriteLine("Got game command");
            game.receiveCommand(msg);
        }

        private void processInitMsg(PacketHeader packetHeader, Connection connection, InitMsg msg)
        {
            Console.WriteLine("Got init message");
            game.InitData(msg);
        }
    }
}